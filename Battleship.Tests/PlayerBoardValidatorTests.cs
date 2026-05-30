using Battleship.Core.Models;
using Battleship.Core.Placement;
using Battleship.Core.Validation;

namespace Battleship.Tests;

public class PlayerBoardValidatorTests
{
    private readonly PlayerBoardValidator _validator = new(new ValidPlacements());

    [Fact]
    public void Validate_AcceptsValidFleet()
    {
        var result = _validator.Validate(TestBoards.ValidFleet());

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorCode);
        Assert.NotNull(result.Board);
        Assert.Equal(5, result.Board!.Ships.Length);
    }

    [Fact]
    public void Validate_BuildsShipsInCanonicalOrder()
    {
        var result = _validator.Validate(TestBoards.ValidFleet());

        var types = result.Board!.Ships.Select(s => s.Type).ToArray();
        Assert.Equal(ShipDefinition.OrderedTypes.ToArray(), types);
    }

    [Fact]
    public void Validate_TotalShipSquaresIs17()
    {
        var result = _validator.Validate(TestBoards.ValidFleet());

        var occupied = 0;
        for (var i = 0; i < BoardBitsCount; i++)
        {
            if ((result.Board!.Occupancy & ((UInt128)1 << i)) != 0)
            {
                occupied++;
            }
        }

        Assert.Equal(17, occupied);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("CCCCC")]
    public void Validate_RejectsWrongLength(string? board)
    {
        var result = _validator.Validate(board);

        Assert.False(result.IsValid);
        Assert.Equal(ValidationErrorCode.InvalidBoardLength, result.ErrorCode);
    }

    [Theory]
    [InlineData('o')]
    [InlineData('x')]
    [InlineData('c')]
    [InlineData('z')]
    public void Validate_RejectsDisallowedCharacters(char bad)
    {
        var board = TestBoards.Empty();
        TestBoards.Place(board, 'C', 0, 5, horizontal: true);
        TestBoards.Place(board, 'B', 10, 4, horizontal: true);
        TestBoards.Place(board, 'D', 20, 3, horizontal: true);
        TestBoards.Place(board, 'S', 30, 3, horizontal: true);
        TestBoards.Place(board, 'P', 40, 2, horizontal: true);
        board[99] = bad;

        var result = _validator.Validate(new string(board));

        Assert.False(result.IsValid);
        Assert.Equal(ValidationErrorCode.InvalidBoard, result.ErrorCode);
    }

    [Fact]
    public void Validate_RejectsMissingShip()
    {
        var board = TestBoards.Empty();
        TestBoards.Place(board, 'C', 0, 5, horizontal: true);
        TestBoards.Place(board, 'B', 10, 4, horizontal: true);
        TestBoards.Place(board, 'D', 20, 3, horizontal: true);
        TestBoards.Place(board, 'S', 30, 3, horizontal: true);
        // Patrol Boat omitted.

        var result = _validator.Validate(new string(board));

        Assert.False(result.IsValid);
        Assert.Equal(ValidationErrorCode.InvalidShipCount, result.ErrorCode);
    }

    [Fact]
    public void Validate_RejectsWrongShipLength()
    {
        var board = TestBoards.Empty();
        TestBoards.Place(board, 'C', 0, 4, horizontal: true); // carrier too short
        TestBoards.Place(board, 'B', 10, 4, horizontal: true);
        TestBoards.Place(board, 'D', 20, 3, horizontal: true);
        TestBoards.Place(board, 'S', 30, 3, horizontal: true);
        TestBoards.Place(board, 'P', 40, 2, horizontal: true);

        var result = _validator.Validate(new string(board));

        Assert.False(result.IsValid);
        Assert.Equal(ValidationErrorCode.InvalidShipCount, result.ErrorCode);
    }

    [Fact]
    public void Validate_RejectsNonContiguousShip()
    {
        // Correct count (5 carrier squares) but scattered -> invalid placement.
        var board = TestBoards.Empty();
        board[0] = 'C';
        board[2] = 'C';
        board[4] = 'C';
        board[6] = 'C';
        board[8] = 'C';
        TestBoards.Place(board, 'B', 10, 4, horizontal: true);
        TestBoards.Place(board, 'D', 20, 3, horizontal: true);
        TestBoards.Place(board, 'S', 30, 3, horizontal: true);
        TestBoards.Place(board, 'P', 40, 2, horizontal: true);

        var result = _validator.Validate(new string(board));

        Assert.False(result.IsValid);
        Assert.Equal(ValidationErrorCode.InvalidShipPlacement, result.ErrorCode);
    }

    [Fact]
    public void Validate_RejectsRowWrappingShip()
    {
        // 3 destroyer squares wrapping from row 0 into row 1 (indices 8,9,10).
        var board = TestBoards.Empty();
        TestBoards.Place(board, 'C', 0, 5, horizontal: true);
        TestBoards.Place(board, 'B', 50, 4, horizontal: true);
        board[8] = 'D';
        board[9] = 'D';
        board[10] = 'D';
        TestBoards.Place(board, 'S', 30, 3, horizontal: true);
        TestBoards.Place(board, 'P', 40, 2, horizontal: true);

        var result = _validator.Validate(new string(board));

        Assert.False(result.IsValid);
        Assert.Equal(ValidationErrorCode.InvalidShipPlacement, result.ErrorCode);
    }

    [Fact]
    public void Validate_AcceptsVerticalPlacement()
    {
        var board = TestBoards.Empty();
        TestBoards.Place(board, 'C', 0, 5, horizontal: false);  // column 0
        TestBoards.Place(board, 'B', 1, 4, horizontal: false);  // column 1
        TestBoards.Place(board, 'D', 2, 3, horizontal: false);  // column 2
        TestBoards.Place(board, 'S', 3, 3, horizontal: false);  // column 3
        TestBoards.Place(board, 'P', 4, 2, horizontal: false);  // column 4

        var result = _validator.Validate(new string(board));

        Assert.True(result.IsValid);
    }

    private const int BoardBitsCount = 100;
}
