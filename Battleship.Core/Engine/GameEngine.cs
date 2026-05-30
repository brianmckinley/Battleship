using Battleship.Core.AI;
using Battleship.Core.Models;
using Battleship.Core.Validation;

namespace Battleship.Core.Engine;

/// <summary>
/// Coordinates the full game lifecycle: create, submit move (with the bot's
/// immediate reply), fetch, and delete. All validation, turn sequencing, and
/// status transitions live here so callers only pass inputs and handle results.
/// </summary>
public sealed class GameEngine
{
    /// <summary>Maximum number of games returned by <see cref="ListGames"/>.</summary>
    public const int DefaultListLimit = 100;

    private readonly PlayerBoardValidator _validator;
    private readonly BotPlacementService _botPlacement;
    private readonly BotTargetingService _botTargeting;
    private readonly MoveProcessor _moveProcessor;
    private readonly GameStore _store;

    public GameEngine(
        PlayerBoardValidator validator,
        BotPlacementService botPlacement,
        BotTargetingService botTargeting,
        MoveProcessor moveProcessor,
        GameStore store)
    {
        _validator = validator;
        _botPlacement = botPlacement;
        _botTargeting = botTargeting;
        _moveProcessor = moveProcessor;
        _store = store;
    }

    /// <summary>
    /// Validates the submitted player board, places the bot fleet, stores the
    /// game, and—when requested—executes the bot's opening move.
    /// </summary>
    public GameOperationResult CreateGame(string? playerBoard, bool botMovesFirst)
    {
        var validation = _validator.Validate(playerBoard);
        if (!validation.IsValid)
        {
            return GameOperationResult.Failure(validation.ErrorCode!, validation.ErrorMessage!);
        }

        var game = new Game
        {
            GameId = Guid.NewGuid(),
            PlayerBoard = validation.Board!,
            BotBoard = _botPlacement.CreateBoard(),
            Status = GameStatus.InProgress,
        };

        if (botMovesFirst)
        {
            ExecuteBotMove(game);
        }

        _store.Add(game);
        return GameOperationResult.Success(game);
    }

    /// <summary>
    /// Applies the player's shot to the bot board, then—unless the player has
    /// won—executes the bot's reply against the player board.
    /// </summary>
    public GameOperationResult SubmitMove(Guid gameId, int location)
    {
        var game = _store.Get(gameId);
        if (game is null)
        {
            return GameOperationResult.Failure(
                ValidationErrorCode.GameNotFound,
                $"Game {gameId} was not found.");
        }

        if (game.Status != GameStatus.InProgress)
        {
            return GameOperationResult.Failure(
                ValidationErrorCode.GameOver,
                "The game is already complete.");
        }

        var playerOutcome = _moveProcessor.ApplyShot(game.BotBoard, location);
        if (!playerOutcome.IsValid)
        {
            return GameOperationResult.Failure(playerOutcome.ErrorCode!, playerOutcome.ErrorMessage!);
        }

        game.LastPlayerMove = playerOutcome.ToMoveResult();

        if (playerOutcome.IsWin)
        {
            game.Status = GameStatus.PlayerWon;
            return GameOperationResult.Success(game);
        }

        ExecuteBotMove(game);
        return GameOperationResult.Success(game);
    }

    public GameOperationResult GetGame(Guid gameId)
    {
        var game = _store.Get(gameId);
        return game is null
            ? GameOperationResult.Failure(
                ValidationErrorCode.GameNotFound,
                $"Game {gameId} was not found.")
            : GameOperationResult.Success(game);
    }

    public bool DeleteGame(Guid gameId) => _store.Remove(gameId);

    /// <summary>Returns up to <paramref name="max"/> stored games (default 100).</summary>
    public IReadOnlyList<Game> ListGames(int max = DefaultListLimit) => _store.List(max);

    private void ExecuteBotMove(Game game)
    {
        var location = _botTargeting.SelectTarget(game.PlayerBoard);
        var outcome = _moveProcessor.ApplyShot(game.PlayerBoard, location);
        game.LastBotMove = outcome.ToMoveResult();

        if (outcome.IsWin)
        {
            game.Status = GameStatus.BotWon;
        }
    }
}
