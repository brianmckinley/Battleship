namespace Battleship.Api.Hosting;

/// <summary>
/// Configuration for the background game-cleanup sweep. Bound from the
/// <c>GameCleanup</c> configuration section.
/// </summary>
public sealed class GameCleanupOptions
{
    public const string SectionName = "GameCleanup";

    /// <summary>How often the cleanup sweep runs.</summary>
    public TimeSpan SweepInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>Finished games are removed once idle this long.</summary>
    public TimeSpan CompletedGameTtl { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>In-progress games are removed once idle this long (abandoned games).</summary>
    public TimeSpan IdleGameTtl { get; set; } = TimeSpan.FromHours(3);
}
