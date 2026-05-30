namespace Battleship.Core.Models;

public class Game
{
    public Guid GameId { get; init; }

    public Board PlayerBoard { get; init; } = new();

    public Board BotBoard { get; init; } = new();

    public GameStatus Status { get; set; } = GameStatus.InProgress;

    public MoveResult? LastPlayerMove { get; set; }

    public MoveResult? LastBotMove { get; set; }

    /// <summary>When the game was created (UTC).</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>When the game was last created or moved on (UTC); drives expiry.</summary>
    public DateTimeOffset LastActivityAt { get; set; }
}
