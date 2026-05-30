using Battleship.Core.Engine;
using Battleship.Core.Placement;
using Battleship.Core.Validation;

namespace Battleship.Tests;

public class MoveProcessorTests
{
    private readonly MoveProcessor _processor = new();
    private readonly PlayerBoardValidator _validator = new(new ValidPlacements());

    private Core.Models.Board BuildFleet() =>
        _validator.Validate(TestBoards.ValidFleet()).Board!;

    [Fact]
    public void ApplyShot_OnEmptyWater_IsMiss()
    {
        var board = BuildFleet();

        var outcome = _processor.ApplyShot(board, 99); // empty square

        Assert.True(outcome.IsValid);
        Assert.Equal(ShotResult.Miss, outcome.Result);
        Assert.Equal(MoveResultValues.Miss, outcome.ToMoveResult().Result);
        Assert.True(board.IsMiss(99));
    }

    [Fact]
    public void ApplyShot_OnShipSegment_IsHit()
    {
        var board = BuildFleet();

        var outcome = _processor.ApplyShot(board, 0); // carrier head

        Assert.Equal(ShotResult.Hit, outcome.Result);
        Assert.True(board.IsHit(0));
    }

    [Fact]
    public void ApplyShot_SinksShip_WhenAllSegmentsHit()
    {
        var board = BuildFleet();

        // Patrol Boat occupies indices 40,41.
        _processor.ApplyShot(board, 40);
        var outcome = _processor.ApplyShot(board, 41);

        Assert.Equal(ShotResult.Sunk, outcome.Result);
        Assert.Equal(Core.Models.ShipType.PatrolBoat, outcome.SunkShip);
    }

    [Fact]
    public void ApplyShot_WinsGame_WhenAllShipsSunk()
    {
        var board = BuildFleet();

        // Hit every occupied square except the final one.
        var occupied = OccupiedLocations(board);
        var last = occupied[^1];

        foreach (var location in occupied[..^1])
        {
            _processor.ApplyShot(board, location);
        }

        var outcome = _processor.ApplyShot(board, last);

        Assert.Equal(ShotResult.Win, outcome.Result);
        Assert.True(outcome.IsWin);
        Assert.Equal(MoveResultValues.Win, outcome.ToMoveResult().Result);
    }

    [Fact]
    public void ApplyShot_RepeatedLocation_Fails()
    {
        var board = BuildFleet();

        _processor.ApplyShot(board, 0);
        var outcome = _processor.ApplyShot(board, 0);

        Assert.False(outcome.IsValid);
        Assert.Equal(ValidationErrorCode.LocationAlreadyPlayed, outcome.ErrorCode);
    }

    [Fact]
    public void ApplyShot_RepeatedMiss_Fails()
    {
        var board = BuildFleet();

        _processor.ApplyShot(board, 99);
        var outcome = _processor.ApplyShot(board, 99);

        Assert.False(outcome.IsValid);
        Assert.Equal(ValidationErrorCode.LocationAlreadyPlayed, outcome.ErrorCode);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100)]
    public void ApplyShot_OutOfRange_Fails(int location)
    {
        var board = BuildFleet();

        var outcome = _processor.ApplyShot(board, location);

        Assert.False(outcome.IsValid);
        Assert.Equal(ValidationErrorCode.InvalidLocation, outcome.ErrorCode);
    }

    [Fact]
    public void ApplyShot_Failure_DoesNotMutateBoard()
    {
        var board = BuildFleet();
        var hitsBefore = board.Hits;
        var missesBefore = board.Misses;

        _processor.ApplyShot(board, 100);

        Assert.Equal(hitsBefore, board.Hits);
        Assert.Equal(missesBefore, board.Misses);
    }

    private static int[] OccupiedLocations(Core.Models.Board board)
    {
        var locations = new List<int>();
        for (var i = 0; i < 100; i++)
        {
            if ((board.Occupancy & ((UInt128)1 << i)) != 0)
            {
                locations.Add(i);
            }
        }

        return locations.ToArray();
    }
}
