namespace Battleship.Core.Models;

/// <summary>
/// A fleet board with ship placements and shot history (hits and misses).
/// </summary>
public class Board
{
    public Ship[] Ships { get; init; } = [];

    public UInt128 Hits { get; set; }

    public UInt128 Misses { get; set; }

    public UInt128 Occupancy
    {
        get
        {
            UInt128 occupancy = 0;
            foreach (var ship in Ships)
            {
                occupancy |= ship.Positions;
            }

            return occupancy;
        }
    }

    public bool IsOccupied(int location) => (Occupancy & BoardBits.LocationMask(location)) != 0;

    public bool IsHit(int location) => (Hits & BoardBits.LocationMask(location)) != 0;

    public bool IsMiss(int location) => (Misses & BoardBits.LocationMask(location)) != 0;

    public bool IsSunk(Ship ship) => (ship.Positions & Hits) == ship.Positions;
}
