using Battleship.Core;
using Battleship.Core.Models;

namespace Battleship.Tests;

public class ShipTests
{
    [Fact]
    public void Create_PacksTypeAndPositions()
    {
        var positions = BoardBits.LocationMask(0)
            | BoardBits.LocationMask(1)
            | BoardBits.LocationMask(2)
            | BoardBits.LocationMask(3)
            | BoardBits.LocationMask(4);

        var ship = Ship.Create(ShipType.Carrier, positions);

        Assert.Equal(ShipType.Carrier, ship.Type);
        Assert.Equal(positions, ship.Positions);
    }
}
