using Battleship.Core.Models;

namespace Battleship.Core.Engine;

/// <summary>
/// Result of attempting a shot. On success it describes the shot result and any
/// ship sunk; on failure it carries an error code and message (spec §9).
/// </summary>
public sealed class MoveOutcome
{
    private MoveOutcome(
        bool isValid,
        int location,
        ShotResult result,
        ShipType? sunkShip,
        string? errorCode,
        string? errorMessage)
    {
        IsValid = isValid;
        Location = location;
        Result = result;
        SunkShip = sunkShip;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool IsValid { get; }

    public int Location { get; }

    public ShotResult Result { get; }

    /// <summary>The ship type sunk by this shot, when <see cref="Result"/> is Sunk or Win.</summary>
    public ShipType? SunkShip { get; }

    public string? ErrorCode { get; }

    public string? ErrorMessage { get; }

    /// <summary>True when this shot ended the game.</summary>
    public bool IsWin => IsValid && Result == ShotResult.Win;

    public MoveResult ToMoveResult() => new()
    {
        Location = Location,
        Result = Result.ToResultValue(),
    };

    public static MoveOutcome Success(int location, ShotResult result, ShipType? sunkShip) =>
        new(true, location, result, sunkShip, null, null);

    public static MoveOutcome Failure(int location, string errorCode, string errorMessage) =>
        new(false, location, default, null, errorCode, errorMessage);
}
