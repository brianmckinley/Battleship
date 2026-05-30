using Battleship.Core.Models;

namespace Battleship.Api.DTOs;

/// <summary>The full game state returned to the client (spec §7).</summary>
public sealed class GameStateResponse
{
    public string GameId { get; set; } = "";

    public string Status { get; set; } = "";

    public string PlayerBoard { get; set; } = "";

    public string OpponentBoard { get; set; } = "";

    public MoveResult? LastPlayerMove { get; set; }

    public MoveResult? LastBotMove { get; set; }
}
