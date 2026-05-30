using Battleship.Core.Models;

namespace Battleship.Core.Engine;

/// <summary>
/// Result of a game operation: the affected <see cref="Game"/> on success, or
/// an error code and message on failure (spec §9).
/// </summary>
public sealed class GameOperationResult
{
    private GameOperationResult(bool isSuccess, Game? game, string? errorCode, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Game = game;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public Game? Game { get; }

    public string? ErrorCode { get; }

    public string? ErrorMessage { get; }

    public static GameOperationResult Success(Game game) =>
        new(true, game, null, null);

    public static GameOperationResult Failure(string errorCode, string errorMessage) =>
        new(false, null, errorCode, errorMessage);
}
