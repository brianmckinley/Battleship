using Battleship.Core.Models;

namespace Battleship.Core.AI;

/// <summary>
/// Selects the bot's next shot using a classic "hunt and target" strategy:
/// <list type="bullet">
/// <item>Hunt mode (no live hits): fire at a random square not yet played.</item>
/// <item>Target mode: when a ship has been hit but not sunk, prefer the squares
/// beside/above/below those hits. When two or more hits line up in a row or
/// column, prioritise extending that line at its open ends.</item>
/// </list>
/// Only hits on ships that are not yet sunk are chased; cells belonging to a
/// sunk ship are ignored, which is fair because sinking is announced in
/// Battleship and prevents the bot from wasting shots around a destroyed ship.
/// </summary>
public sealed class BotTargetingService
{
    private const int LastColumn = BoardBits.GridDimension - 1;
    private const int LastRow = BoardBits.GridDimension - 1;
    private const int RowStep = BoardBits.GridDimension;

    private readonly Random _random;

    public BotTargetingService(Random? random = null)
    {
        _random = random ?? Random.Shared;
    }

    /// <summary>
    /// Returns the next location (0–99) to fire at on <paramref name="targetBoard"/>.
    /// Throws if the board has no remaining targets.
    /// </summary>
    public int SelectTarget(Board targetBoard)
    {
        ArgumentNullException.ThrowIfNull(targetBoard);

        var played = targetBoard.Hits | targetBoard.Misses;

        var available = BuildLocations(BoardBits.PositionMask & ~played);
        if (available.Count == 0)
        {
            throw new InvalidOperationException("No available target locations remain.");
        }

        var activeHits = ComputeActiveHits(targetBoard);
        if (activeHits == 0)
        {
            return Pick(available);
        }

        var activeLocations = BuildLocations(activeHits);

        // Highest priority: continue an established line of two or more hits.
        var lineCandidates = BuildLineExtensionCandidates(activeHits, played, activeLocations);
        if (lineCandidates.Count > 0)
        {
            return Pick(lineCandidates);
        }

        // Next: the orthogonal neighbours of an isolated hit.
        var neighbourCandidates = BuildNeighbourCandidates(played, activeLocations);
        if (neighbourCandidates.Count > 0)
        {
            return Pick(neighbourCandidates);
        }

        return Pick(available);
    }

    private int Pick(IReadOnlyList<int> candidates) => candidates[_random.Next(candidates.Count)];

    /// <summary>
    /// All hit cells excluding those belonging to ships that are already sunk.
    /// </summary>
    private static UInt128 ComputeActiveHits(Board board)
    {
        var activeHits = board.Hits;
        foreach (var ship in board.Ships)
        {
            if (board.IsSunk(ship))
            {
                activeHits &= ~ship.Positions;
            }
        }

        return activeHits;
    }

    /// <summary>
    /// Open ends of every horizontal/vertical run of two or more aligned active hits.
    /// </summary>
    private static List<int> BuildLineExtensionCandidates(UInt128 activeHits, UInt128 played, IReadOnlyList<int> activeLocations)
    {
        var candidates = new SortedSet<int>();

        // Horizontal lines: step by one column, staying within the same row.
        AddLineExtensions(activeHits, played, activeLocations, candidates, step: 1,
            canMoveNegative: static location => Column(location) > 0,
            canMovePositive: static location => Column(location) < LastColumn);

        // Vertical lines: step by one row, staying within the same column.
        AddLineExtensions(activeHits, played, activeLocations, candidates, step: RowStep,
            canMoveNegative: static location => Row(location) > 0,
            canMovePositive: static location => Row(location) < LastRow);

        return [.. candidates];
    }

    private static void AddLineExtensions(
        UInt128 activeHits,
        UInt128 played,
        IReadOnlyList<int> activeLocations,
        SortedSet<int> candidates,
        int step,
        Func<int, bool> canMoveNegative,
        Func<int, bool> canMovePositive)
    {
        foreach (var location in activeLocations)
        {
            var hasNegativeNeighbour = canMoveNegative(location) && IsSet(activeHits, location - step);
            var hasPositiveNeighbour = canMovePositive(location) && IsSet(activeHits, location + step);
            if (!hasNegativeNeighbour && !hasPositiveNeighbour)
            {
                continue;
            }

            AddRunEnd(activeHits, played, candidates, location, -step, canMoveNegative);
            AddRunEnd(activeHits, played, candidates, location, step, canMovePositive);
        }
    }

    private static void AddRunEnd(
        UInt128 activeHits,
        UInt128 played,
        SortedSet<int> candidates,
        int start,
        int step,
        Func<int, bool> canMove)
    {
        var end = start;
        while (canMove(end) && IsSet(activeHits, end + step))
        {
            end += step;
        }

        if (canMove(end))
        {
            var extension = end + step;
            if (!IsSet(played, extension))
            {
                candidates.Add(extension);
            }
        }
    }

    /// <summary>
    /// The unplayed squares directly beside, above, and below the active hits.
    /// </summary>
    private static List<int> BuildNeighbourCandidates(UInt128 played, IReadOnlyList<int> activeLocations)
    {
        var candidates = new SortedSet<int>();
        foreach (var location in activeLocations)
        {
            AddNeighbour(played, candidates, location, Column(location) > 0, -1);
            AddNeighbour(played, candidates, location, Column(location) < LastColumn, 1);
            AddNeighbour(played, candidates, location, Row(location) > 0, -RowStep);
            AddNeighbour(played, candidates, location, Row(location) < LastRow, RowStep);
        }

        return [.. candidates];
    }

    private static void AddNeighbour(UInt128 played, SortedSet<int> candidates, int location, bool inBounds, int delta)
    {
        if (!inBounds)
        {
            return;
        }

        var neighbour = location + delta;
        if (!IsSet(played, neighbour))
        {
            candidates.Add(neighbour);
        }
    }

    private static List<int> BuildLocations(UInt128 bits)
    {
        var locations = new List<int>(BoardBits.BoardSize);
        for (var location = 0; location < BoardBits.BoardSize; location++)
        {
            if (IsSet(bits, location))
            {
                locations.Add(location);
            }
        }

        return locations;
    }

    private static bool IsSet(UInt128 bits, int location) => (bits & BoardBits.LocationMask(location)) != 0;

    private static int Row(int location) => location / BoardBits.GridDimension;

    private static int Column(int location) => location % BoardBits.GridDimension;
}
