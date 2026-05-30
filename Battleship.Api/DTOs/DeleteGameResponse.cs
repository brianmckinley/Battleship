namespace Battleship.Api.DTOs;

/// <summary>Response for a successful game deletion (spec §8).</summary>
public sealed class DeleteGameResponse
{
    public bool Success { get; set; }
}
