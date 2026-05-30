using Battleship.Core;
using Battleship.Core.Engine;
using Battleship.Core.Placement;
using Battleship.Core.Serialization;
using Battleship.Core.Validation;

namespace Battleship.Tests;

public class BoardSerializerTests
{
    private readonly PlayerBoardValidatorHarness _harness = new();

    [Fact]
    public void PlayerView_FreshFleet_ShowsAllShipsUppercase()
    {
        var board = _harness.Fleet();

        var view = BoardSerializer.SerializePlayerView(board);

        Assert.Equal(100, view.Length);
        Assert.Equal("CCCCC.....", view[..10]);   // row 0
        Assert.Equal("BBBB......", view[10..20]);  // row 1
        Assert.Equal("DDD.......", view[20..30]);  // row 2
        Assert.Equal("SSS.......", view[30..40]);  // row 3
        Assert.Equal("PP........", view[40..50]);  // row 4
    }

    [Fact]
    public void PlayerView_ShowsDamageAndMisses()
    {
        var board = _harness.Fleet();
        board.Hits |= BoardBits.LocationMask(0);   // carrier hit
        board.Misses |= BoardBits.LocationMask(99); // bot miss

        var view = BoardSerializer.SerializePlayerView(board);

        Assert.Equal('c', view[0]);
        Assert.Equal('C', view[1]);
        Assert.Equal('o', view[99]);
    }

    [Fact]
    public void OpponentView_FreshFleet_IsAllHidden()
    {
        var board = _harness.Fleet();

        var view = BoardSerializer.SerializeOpponentView(board);

        Assert.Equal(new string('.', 100), view);
    }

    [Fact]
    public void OpponentView_UnsunkHit_ShowsX_AndHidesRest()
    {
        var board = _harness.Fleet();
        board.Hits |= BoardBits.LocationMask(0); // carrier, not sunk

        var view = BoardSerializer.SerializeOpponentView(board);

        Assert.Equal('x', view[0]);
        Assert.Equal('.', view[1]); // remaining carrier segments stay hidden
    }

    [Fact]
    public void OpponentView_Miss_ShowsO()
    {
        var board = _harness.Fleet();
        board.Misses |= BoardBits.LocationMask(50);

        var view = BoardSerializer.SerializeOpponentView(board);

        Assert.Equal('o', view[50]);
    }

    [Fact]
    public void OpponentView_SunkShip_RevealsShipCharacter()
    {
        var board = _harness.Fleet();
        var processor = new MoveProcessor();

        // Patrol Boat at 40,41.
        processor.ApplyShot(board, 40);
        processor.ApplyShot(board, 41);

        var view = BoardSerializer.SerializeOpponentView(board);

        Assert.Equal('P', view[40]);
        Assert.Equal('P', view[41]);
    }

    private sealed class PlayerBoardValidatorHarness
    {
        private readonly PlayerBoardValidator _validator = new(new ValidPlacements());

        public Core.Models.Board Fleet() => _validator.Validate(TestBoards.ValidFleet()).Board!;
    }
}
