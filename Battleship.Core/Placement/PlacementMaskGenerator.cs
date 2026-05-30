using Battleship.Core.Models;
using Battleship.Core.Validation;

namespace Battleship.Core.Placement;

/// <summary>
/// Generates all valid ship placement masks programmatically (not hard-coded).
/// </summary>
public static class PlacementMaskGenerator
{
    public static Dictionary<ShipType, HashSet<UInt128>> GenerateLookup()
    {
        var result = new Dictionary<ShipType, HashSet<UInt128>>();

        foreach (var definition in ShipDefinition.Catalog)
        {
            result[definition.Type] = GenerateMasksForLength(definition.Length);
        }

        return result;
    }

    public static Dictionary<ShipType, UInt128[]> GenerateArrays()
    {
        var lookup = GenerateLookup();
        return lookup.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToArray());
    }

    private static HashSet<UInt128> GenerateMasksForLength(int length)
    {
        var masks = new HashSet<UInt128>();

        for (var row = 0; row < BoardBits.GridDimension; row++)
        {
            for (var col = 0; col <= BoardBits.GridDimension - length; col++)
            {
                masks.Add(BuildHorizontalMask(row, col, length));
            }
        }

        for (var col = 0; col < BoardBits.GridDimension; col++)
        {
            for (var row = 0; row <= BoardBits.GridDimension - length; row++)
            {
                masks.Add(BuildVerticalMask(row, col, length));
            }
        }

        return masks;
    }

    private static UInt128 BuildHorizontalMask(int row, int startCol, int length)
    {
        UInt128 mask = 0;
        var baseIndex = row * BoardBits.GridDimension;

        for (var i = 0; i < length; i++)
        {
            mask |= BoardBits.LocationMask(baseIndex + startCol + i);
        }

        return mask;
    }

    private static UInt128 BuildVerticalMask(int startRow, int col, int length)
    {
        UInt128 mask = 0;

        for (var i = 0; i < length; i++)
        {
            var index = (startRow + i) * BoardBits.GridDimension + col;
            mask |= BoardBits.LocationMask(index);
        }

        return mask;
    }
}
