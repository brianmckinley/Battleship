namespace Battleship.Core.Validation;

/// <summary>
/// Constants for create-game and move validation per the specification.
/// </summary>
public static class BoardValidationConstants
{
    public const int BoardStringLength = 100;
    public const int TotalShipSquares = 17;
    public const int MinLocation = 0;
    public const int MaxLocation = 99;

    public const char EmptyWater = '.';
    public const char MissMarker = 'o';
    public const char UnsunkHitMarker = 'x';

    /// <summary>Allowed characters in a submitted player board at game creation.</summary>
    public static readonly HashSet<char> AllowedPlacementCharacters =
    [
        EmptyWater,
        'C',
        'B',
        'D',
        'S',
        'P',
    ];

    /// <summary>Disallowed at game creation (damage, misses, lowercase ship markers).</summary>
    public static readonly HashSet<char> DisallowedPlacementCharacters =
    [
        MissMarker,
        UnsunkHitMarker,
        'c',
        'b',
        'd',
        's',
        'p',
    ];
}
