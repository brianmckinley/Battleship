using Battleship.Core.Models;

namespace Battleship.Core.Placement;

/// <summary>
/// Precomputed valid placement masks, initialized at application startup.
/// </summary>
public sealed class ValidPlacements
{
    public ValidPlacements()
    {
        Lookup = PlacementMaskGenerator.GenerateLookup();
        Arrays = PlacementMaskGenerator.GenerateArrays();
    }

    public IReadOnlyDictionary<ShipType, HashSet<UInt128>> Lookup { get; }

    public IReadOnlyDictionary<ShipType, UInt128[]> Arrays { get; }

    public bool IsValidPlacement(ShipType type, UInt128 mask) =>
        Lookup[type].Contains(mask & BoardBits.PositionMask);
}
