namespace Battleship.Api.DTOs;

/// <summary>Request body for submitting a move; <see cref="Location"/> is 0–99.</summary>
public sealed class MoveRequest
{
    public int Location { get; set; }
}
