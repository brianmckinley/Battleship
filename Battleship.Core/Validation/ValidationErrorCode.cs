namespace Battleship.Core.Validation;

/// <summary>
/// Error codes returned by validation, matching the API contract (spec §9).
/// </summary>
public static class ValidationErrorCode
{
    public const string InvalidBoard = "InvalidBoard";
    public const string InvalidBoardLength = "InvalidBoardLength";
    public const string InvalidShipCount = "InvalidShipCount";
    public const string InvalidShipPlacement = "InvalidShipPlacement";
    public const string InvalidLocation = "InvalidLocation";
    public const string LocationAlreadyPlayed = "LocationAlreadyPlayed";
    public const string GameNotFound = "GameNotFound";
    public const string GameOver = "GameOver";
}
