using Battleship.Core.Validation;

namespace Battleship.Core.Engine;

/// <summary>
/// Outcome of a single shot, mapped to the API move-result strings (spec §6).
/// </summary>
public enum ShotResult
{
    Miss,
    Hit,
    Sunk,
    Win,
}

public static class ShotResultExtensions
{
    public static string ToResultValue(this ShotResult result) => result switch
    {
        ShotResult.Miss => MoveResultValues.Miss,
        ShotResult.Hit => MoveResultValues.Hit,
        ShotResult.Sunk => MoveResultValues.Sunk,
        ShotResult.Win => MoveResultValues.Win,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null),
    };
}
