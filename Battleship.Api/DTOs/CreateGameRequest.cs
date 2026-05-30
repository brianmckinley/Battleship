namespace Battleship.Api.DTOs;

/// <summary>Request body for creating a new game (spec §7).</summary>
public sealed class CreateGameRequest
{
    public string PlayerBoard { get; set; } = "";

    public bool BotMovesFirst { get; set; }
}
