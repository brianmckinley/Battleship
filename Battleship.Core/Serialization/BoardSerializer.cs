using Battleship.Core.Models;
using Battleship.Core.Validation;

namespace Battleship.Core.Serialization;

/// <summary>
/// Generates the 100-character API board strings from internal bitmap state.
/// Board strings are derived on demand and are never the source of truth.
/// </summary>
public static class BoardSerializer
{
    /// <summary>
    /// Player's own-fleet view: ships plus the opponent's shots against it.
    /// Reveals every ship segment (uppercase intact, lowercase damaged).
    /// </summary>
    public static string SerializePlayerView(Board board)
    {
        ArgumentNullException.ThrowIfNull(board);

        var owners = BuildOwnerLookup(board);
        var chars = new char[BoardBits.BoardSize];

        for (var i = 0; i < BoardBits.BoardSize; i++)
        {
            var shot = BoardBits.LocationMask(i);

            if ((board.Misses & shot) != 0)
            {
                chars[i] = BoardValidationConstants.MissMarker;
                continue;
            }

            if (owners[i] is { } type)
            {
                var definition = ShipDefinition.For(type);
                var isHit = (board.Hits & shot) != 0;
                chars[i] = isHit ? definition.DamagedCharacter : definition.PlacementCharacter;
                continue;
            }

            chars[i] = BoardValidationConstants.EmptyWater;
        }

        return new string(chars);
    }

    /// <summary>
    /// Opponent view as known to the firing player: misses, unsunk hits as
    /// <c>x</c>, and revealed ship characters only once a ship is sunk.
    /// </summary>
    public static string SerializeOpponentView(Board board)
    {
        ArgumentNullException.ThrowIfNull(board);

        var owners = BuildOwnerLookup(board);
        var sunk = BuildSunkLookup(board);
        var chars = new char[BoardBits.BoardSize];

        for (var i = 0; i < BoardBits.BoardSize; i++)
        {
            var shot = BoardBits.LocationMask(i);

            if ((board.Misses & shot) != 0)
            {
                chars[i] = BoardValidationConstants.MissMarker;
                continue;
            }

            var isHit = (board.Hits & shot) != 0;

            if (isHit && owners[i] is { } type)
            {
                chars[i] = sunk[(int)type]
                    ? ShipDefinition.For(type).OpponentSunkCharacter
                    : BoardValidationConstants.UnsunkHitMarker;
                continue;
            }

            // Unhit squares stay hidden, whether water or an undiscovered ship.
            chars[i] = BoardValidationConstants.EmptyWater;
        }

        return new string(chars);
    }

    private static ShipType?[] BuildOwnerLookup(Board board)
    {
        var owners = new ShipType?[BoardBits.BoardSize];

        foreach (var ship in board.Ships)
        {
            for (var i = 0; i < BoardBits.BoardSize; i++)
            {
                if ((ship.Positions & BoardBits.LocationMask(i)) != 0)
                {
                    owners[i] = ship.Type;
                }
            }
        }

        return owners;
    }

    private static bool[] BuildSunkLookup(Board board)
    {
        // Indexed by (int)ShipType; type values are 1..5.
        var sunk = new bool[ShipDefinition.Catalog.Count + 2];

        foreach (var ship in board.Ships)
        {
            sunk[(int)ship.Type] = board.IsSunk(ship);
        }

        return sunk;
    }
}
