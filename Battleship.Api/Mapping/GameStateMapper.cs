using Battleship.Api.DTOs;
using Battleship.Core.Engine;
using Battleship.Core.Models;
using Battleship.Core.Serialization;

namespace Battleship.Api.Mapping;

/// <summary>
/// Builds the API <see cref="GameStateResponse"/> from internal game state,
/// deriving board strings on demand (spec §24).
/// </summary>
public static class GameStateMapper
{
    public static GameStateResponse ToResponse(Game game) => new()
    {
        GameId = game.GameId.ToString(),
        Status = game.Status.ToStatusValue(),
        PlayerBoard = BoardSerializer.SerializePlayerView(game.PlayerBoard),
        OpponentBoard = BoardSerializer.SerializeOpponentView(game.BotBoard),
        LastPlayerMove = game.LastPlayerMove,
        LastBotMove = game.LastBotMove,
    };

    public static GameSummaryResponse ToSummary(Game game) => new()
    {
        GameId = game.GameId.ToString(),
        Status = game.Status.ToStatusValue(),
    };
}
