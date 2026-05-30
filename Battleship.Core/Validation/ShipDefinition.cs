using Battleship.Core.Models;

namespace Battleship.Core.Validation;

/// <summary>
/// Per-ship placement and encoding requirements from the specification.
/// </summary>
public sealed record ShipDefinition(
    ShipType Type,
    char PlacementCharacter,
    char DamagedCharacter,
    char OpponentSunkCharacter,
    int Length,
    int RequiredCount)
{
    public static ShipDefinition For(ShipType type) =>
        Catalog.First(s => s.Type == type);

    public static IReadOnlyList<ShipDefinition> Catalog { get; } =
    [
        new(ShipType.Carrier, 'C', 'c', 'C', 5, 1),
        new(ShipType.Battleship, 'B', 'b', 'B', 4, 1),
        new(ShipType.Destroyer, 'D', 'd', 'D', 3, 1),
        new(ShipType.Submarine, 'S', 's', 'S', 3, 1),
        new(ShipType.PatrolBoat, 'P', 'p', 'P', 2, 1),
    ];

    /// <summary>Ship types in canonical storage order.</summary>
    public static IReadOnlyList<ShipType> OrderedTypes { get; } =
    [
        ShipType.Carrier,
        ShipType.Battleship,
        ShipType.Destroyer,
        ShipType.Submarine,
        ShipType.PatrolBoat,
    ];
}
