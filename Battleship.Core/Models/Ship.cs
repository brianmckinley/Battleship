namespace Battleship.Core.Models;

/// <summary>
/// A ship represented as a single UInt128: positions in bits 0–99, type in bits 100–102.
/// </summary>
public struct Ship
{
    public UInt128 Data { get; set; }

    public UInt128 Positions => Data & BoardBits.PositionMask;

    public ShipType Type => (ShipType)((int)((Data >> BoardBits.ShipTypeShift) & 0b111));

    public static Ship Create(ShipType type, UInt128 positions)
    {
        var data = (positions & BoardBits.PositionMask) | ((UInt128)(int)type << BoardBits.ShipTypeShift);
        return new Ship { Data = data };
    }
}
