using Battleship.Core;

namespace Battleship.Tests;

public class BoardBitsTests
{
    [Fact]
    public void PositionMask_HasExactly100BitsSet()
    {
        var count = 0;
        var mask = BoardBits.PositionMask;

        for (var i = 0; i < BoardBits.BoardSize; i++)
        {
            if ((mask & BoardBits.LocationMask(i)) != 0)
            {
                count++;
            }
        }

        Assert.Equal(100, count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(99)]
    public void LocationMask_SetsSingleBit(int location)
    {
        var mask = BoardBits.LocationMask(location);

        Assert.NotEqual((UInt128)0, mask & BoardBits.PositionMask);
        Assert.Equal(mask, mask & BoardBits.PositionMask);
    }

    [Fact]
    public void LocationMask_RejectsOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BoardBits.LocationMask(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => BoardBits.LocationMask(100));
    }
}
