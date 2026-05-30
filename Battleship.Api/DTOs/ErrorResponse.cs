namespace Battleship.Api.DTOs;

/// <summary>Standard error payload (spec §9).</summary>
public sealed class ErrorResponse
{
    public string Message { get; set; } = "";

    public string Code { get; set; } = "";
}
