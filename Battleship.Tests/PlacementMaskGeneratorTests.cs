using Battleship.Core.Models;
using Battleship.Core.Placement;
using Battleship.Core.Validation;

namespace Battleship.Tests;

public class PlacementMaskGeneratorTests
{
    [Theory]
    [InlineData(ShipType.Carrier, 5, 120)]
    [InlineData(ShipType.Battleship, 4, 140)]
    [InlineData(ShipType.Destroyer, 3, 160)]
    [InlineData(ShipType.Submarine, 3, 160)]
    [InlineData(ShipType.PatrolBoat, 2, 180)]
    public void GenerateLookup_HasExpectedPlacementCount(ShipType type, int length, int expectedCount)
    {
        var lookup = PlacementMaskGenerator.GenerateLookup();

        Assert.Equal(expectedCount, lookup[type].Count);
        Assert.Equal(PlacementConstants.TotalPlacementsForLength(length), lookup[type].Count);
    }

    [Fact]
    public void ValidPlacements_ContainsKnownCarrierPlacement()
    {
        var placements = new ValidPlacements();
        var mask = ((UInt128)1 << 2) | ((UInt128)1 << 3) | ((UInt128)1 << 4)
            | ((UInt128)1 << 5) | ((UInt128)1 << 6);

        Assert.True(placements.IsValidPlacement(ShipType.Carrier, mask));
    }

    [Fact]
    public void ShipDefinition_CatalogMatchesSpecification()
    {
        Assert.Equal(5, ShipDefinition.Catalog.Count);
        Assert.Equal(17, ShipDefinition.Catalog.Sum(s => s.Length * s.RequiredCount));
        Assert.Equal(BoardValidationConstants.TotalShipSquares, ShipDefinition.Catalog.Sum(s => s.Length * s.RequiredCount));
    }
}
