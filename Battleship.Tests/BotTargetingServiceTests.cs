using Battleship.Core;
using Battleship.Core.AI;
using Battleship.Core.Engine;
using Battleship.Core.Models;

namespace Battleship.Tests;

public class BotTargetingServiceTests
{
    [Fact]
    public void SelectTarget_ReturnsLocationInRange()
    {
        var bot = new BotTargetingService(new Random(1));
        var board = new Board();

        var location = bot.SelectTarget(board);

        Assert.InRange(location, 0, 99);
    }

    [Fact]
    public void SelectTarget_NeverPicksAPlayedLocation()
    {
        var bot = new BotTargetingService(new Random(7));
        var board = new Board();

        // Mark all but one location as played; the bot must pick the remaining one.
        for (var i = 0; i < BoardBits.BoardSize; i++)
        {
            if (i != 73)
            {
                board.Misses |= BoardBits.LocationMask(i);
            }
        }

        Assert.Equal(73, bot.SelectTarget(board));
    }

    [Fact]
    public void SelectTarget_NeverRepeats_OverAFullGame()
    {
        var bot = new BotTargetingService(new Random(99));
        var processor = new MoveProcessor();
        var board = new Board(); // no ships -> every shot is a miss

        var seen = new HashSet<int>();

        for (var turn = 0; turn < BoardBits.BoardSize; turn++)
        {
            var location = bot.SelectTarget(board);

            Assert.True(seen.Add(location), $"Location {location} was selected twice.");

            var outcome = processor.ApplyShot(board, location);
            Assert.True(outcome.IsValid);
        }

        Assert.Equal(BoardBits.BoardSize, seen.Count);
    }

    [Fact]
    public void SelectTarget_Throws_WhenBoardFullyPlayed()
    {
        var bot = new BotTargetingService(new Random(3));
        var board = new Board { Misses = BoardBits.PositionMask };

        Assert.Throws<InvalidOperationException>(() => bot.SelectTarget(board));
    }

    [Fact]
    public void SelectTarget_IsDeterministicForSameSeed()
    {
        var board = new Board();

        var first = new BotTargetingService(new Random(50)).SelectTarget(board);
        var second = new BotTargetingService(new Random(50)).SelectTarget(board);

        Assert.Equal(first, second);
    }

    [Fact]
    public void SelectTarget_TargetsAdjacentSquares_AfterAnIsolatedHit()
    {
        var bot = new BotTargetingService(new Random(11));
        // A live (unsunk) hit at 44 is created by placing a longer ship there.
        var board = BoardWithShip(ShipType.Submarine, 44, 45, 46);
        board.Hits = BoardBits.LocationMask(44);

        var location = bot.SelectTarget(board);

        // 34 (up), 54 (down), 43 (left), 45 (right) are the only valid neighbours.
        Assert.Contains(location, new[] { 34, 54, 43, 45 });
    }

    [Fact]
    public void SelectTarget_NeverPicksADiagonalOrDistantSquare_ForAnIsolatedHit()
    {
        var board = BoardWithShip(ShipType.Submarine, 44, 45, 46);
        board.Hits = BoardBits.LocationMask(44);

        var neighbours = new[] { 34, 54, 43, 45 };
        for (var seed = 0; seed < 200; seed++)
        {
            var location = new BotTargetingService(new Random(seed)).SelectTarget(board);
            Assert.Contains(location, neighbours);
        }
    }

    [Fact]
    public void SelectTarget_ExtendsTheLine_WhenTwoHitsAlignHorizontally()
    {
        var board = BoardWithShip(ShipType.Battleship, 44, 45, 46, 47);
        board.Hits = BoardBits.LocationMask(45) | BoardBits.LocationMask(46);

        // Only the open ends of the run (44 and 47) should ever be chosen,
        // never the vertical neighbours of the individual hits.
        var lineEnds = new[] { 44, 47 };
        for (var seed = 0; seed < 200; seed++)
        {
            var location = new BotTargetingService(new Random(seed)).SelectTarget(board);
            Assert.Contains(location, lineEnds);
        }
    }

    [Fact]
    public void SelectTarget_ExtendsTheLine_WhenTwoHitsAlignVertically()
    {
        var board = BoardWithShip(ShipType.Battleship, 35, 45, 55, 65);
        board.Hits = BoardBits.LocationMask(45) | BoardBits.LocationMask(55);

        var lineEnds = new[] { 35, 65 };
        for (var seed = 0; seed < 200; seed++)
        {
            var location = new BotTargetingService(new Random(seed)).SelectTarget(board);
            Assert.Contains(location, lineEnds);
        }
    }

    [Fact]
    public void SelectTarget_SkipsAClosedLineEnd_ThatWasAlreadyPlayed()
    {
        var board = BoardWithShip(ShipType.Battleship, 44, 45, 46, 47);
        board.Hits = BoardBits.LocationMask(45) | BoardBits.LocationMask(46);
        board.Misses = BoardBits.LocationMask(44); // left end blocked

        for (var seed = 0; seed < 50; seed++)
        {
            var location = new BotTargetingService(new Random(seed)).SelectTarget(board);
            Assert.Equal(47, location);
        }
    }

    [Fact]
    public void SelectTarget_IgnoresHitsFromSunkShips_AndReturnsToHunting()
    {
        // A single-cell-equivalent: a destroyer fully hit (sunk) should not be chased.
        var board = BoardWithShip(ShipType.Destroyer, 44, 45);
        board.Hits = BoardBits.LocationMask(44) | BoardBits.LocationMask(45);

        Assert.True(board.IsSunk(board.Ships[0]));

        // With no live hits, the bot hunts; neighbours of the sunk ship are not
        // privileged, so over many seeds it picks a variety of squares.
        var picks = new HashSet<int>();
        for (var seed = 0; seed < 200; seed++)
        {
            picks.Add(new BotTargetingService(new Random(seed)).SelectTarget(board));
        }

        Assert.True(picks.Count > 4, "Expected hunt mode to spread shots across the board.");
    }

    private static Board BoardWithShip(ShipType type, params int[] locations)
    {
        UInt128 positions = 0;
        foreach (var location in locations)
        {
            positions |= BoardBits.LocationMask(location);
        }

        return new Board { Ships = [Ship.Create(type, positions)] };
    }
}
