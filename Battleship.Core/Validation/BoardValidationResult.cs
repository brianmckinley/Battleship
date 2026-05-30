using Battleship.Core.Models;

namespace Battleship.Core.Validation;

/// <summary>
/// Outcome of validating and parsing a submitted player board. On success it
/// carries the constructed <see cref="Board"/>; on failure it carries an error
/// code and message suitable for an API error response.
/// </summary>
public sealed class BoardValidationResult
{
    private BoardValidationResult(Board? board, string? errorCode, string? errorMessage)
    {
        Board = board;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool IsValid => Board is not null;

    public Board? Board { get; }

    public string? ErrorCode { get; }

    public string? ErrorMessage { get; }

    public static BoardValidationResult Success(Board board) =>
        new(board, null, null);

    public static BoardValidationResult Failure(string errorCode, string errorMessage) =>
        new(null, errorCode, errorMessage);
}
