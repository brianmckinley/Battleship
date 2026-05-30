using Battleship.Core.Validation;

namespace Battleship.Core.Placement;

/// <summary>
/// Constants for programmatic placement-mask generation at startup.
/// </summary>
public static class PlacementConstants
{
    /// <summary>
    /// Valid horizontal or vertical placement count: 10 * (11 - length).
    /// </summary>
    public static int PlacementCountForLength(int length) =>
        10 * (11 - length);

    public static IReadOnlyDictionary<Models.ShipType, int> ExpectedPlacementCounts { get; } =
        ShipDefinition.Catalog.ToDictionary(
            s => s.Type,
            s => PlacementCountForLength(s.Length));

    /// <summary>Total valid placements per ship type (horizontal + vertical).</summary>
    public static int TotalPlacementsForLength(int length) =>
        2 * PlacementCountForLength(length);
}
