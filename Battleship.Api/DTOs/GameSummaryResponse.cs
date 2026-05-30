namespace Battleship.Api.DTOs;

/// <summary>A lightweight game listing entry: id and status only.</summary>
public sealed class GameSummaryResponse
{
    public string GameId { get; set; } = "";

    public string Status { get; set; } = "";
}
