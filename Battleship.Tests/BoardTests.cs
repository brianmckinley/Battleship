using Battleship.Core;
using Battleship.Core.Models;

namespace Battleship.Tests;

public class BoardTests
{
    [Fact]
    public void Occupancy_AggregatesAllShipPositions()
    {
        var carrier = Ship.Create(
            ShipType.Carrier,
            BoardBits.LocationMask(0) | BoardBits.LocationMask(1) | BoardBits.LocationMask(2)
                | BoardBits.LocationMask(3) | BoardBits.LocationMask(4));

        var board = new Board
        {
            Ships = [carrier],
        };

        Assert.True(board.IsOccupied(0));
        Assert.True(board.IsOccupied(4));
        Assert.False(board.IsOccupied(5));
    }

    [Fact]
    public void IsSunk_WhenAllPositionsHit()
    {
        var ship = Ship.Create(ShipType.PatrolBoat, BoardBits.LocationMask(0) | BoardBits.LocationMask(1));
        var board = new Board
        {
            Ships = [ship],
            Hits = BoardBits.LocationMask(0) | BoardBits.LocationMask(1),
        };

        Assert.True(board.IsSunk(ship));
    }
}
