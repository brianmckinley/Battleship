using Battleship.Core.Validation;

namespace Battleship.Tests;

/// <summary>
/// Helpers for building 100-character board strings in tests.
/// </summary>
internal static class TestBoards
{
    public const int Length = 100;

    public static char[] Empty()
    {
        var board = new char[Length];
        Array.Fill(board, BoardValidationConstants.EmptyWater);
        return board;
    }

    public static char[] Place(char[] board, char character, int start, int count, bool horizontal)
    {
        for (var i = 0; i < count; i++)
        {
            var index = horizontal ? start + i : start + (i * 10);
            board[index] = character;
        }

        return board;
    }

    /// <summary>
    /// A fully valid, non-overlapping fleet (one ship per row, all horizontal).
    /// </summary>
    public static string ValidFleet()
    {
        var board = Empty();
        Place(board, 'C', 0, 5, horizontal: true);   // row 0
        Place(board, 'B', 10, 4, horizontal: true);  // row 1
        Place(board, 'D', 20, 3, horizontal: true);  // row 2
        Place(board, 'S', 30, 3, horizontal: true);  // row 3
        Place(board, 'P', 40, 2, horizontal: true);  // row 4
        return new string(board);
    }
}
