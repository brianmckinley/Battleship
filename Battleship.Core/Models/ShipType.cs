namespace Battleship.Core.Models;

/// <summary>
/// Ship types stored in fixed order: Carrier through Patrol Boat.
/// </summary>
public enum ShipType
{
    Carrier = 1,
    Battleship = 2,
    Destroyer = 3,
    Submarine = 4,
    PatrolBoat = 5,
}
