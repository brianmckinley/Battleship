namespace Battleship.Core.Models;

public class Game
{
    public Guid GameId { get; init; }

    public Board PlayerBoard { get; init; } = new();

    public Board BotBoard { get; init; } = new();

    public GameStatus Status { get; set; } = GameStatus.InProgress;

    public MoveResult? LastPlayerMove { get; set; }

    public MoveResult? LastBotMove { get; set; }
}
