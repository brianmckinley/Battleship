using Battleship.Core.Models;
using Battleship.Core.Validation;

namespace Battleship.Core.Engine;

public static class GameStatusExtensions
{
    public static string ToStatusValue(this GameStatus status) => status switch
    {
        GameStatus.InProgress => GameStatusValues.InProgress,
        GameStatus.PlayerWon => GameStatusValues.PlayerWon,
        GameStatus.BotWon => GameStatusValues.BotWon,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
    };
}
