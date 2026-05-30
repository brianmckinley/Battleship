namespace Battleship.Core;

/// <summary>
/// Bitmap constants for the 10×10 board (bits 0–99) and ship metadata (bits 100+).
/// </summary>
public static class BoardBits
{
    public const int BoardSize = 100;
    public const int GridDimension = 10;
    public const int ShipTypeShift = 100;
    public const int MinLocation = 0;
    public const int MaxLocation = 99;

    public static readonly UInt128 PositionMask = CreatePositionMask();

    public static UInt128 LocationMask(int location)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(location);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(location, MaxLocation);

        return (UInt128)1 << location;
    }

    private static UInt128 CreatePositionMask()
    {
        // Shift-by-100 can be problematic on some runtimes; set bits 0–99 explicitly.
        UInt128 mask = 0;
        for (var i = 0; i < BoardSize; i++)
        {
            mask |= (UInt128)1 << i;
        }

        return mask;
    }
}
