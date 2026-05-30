using Battleship.Core.Engine;
using Microsoft.Extensions.Options;

namespace Battleship.Api.Hosting;

/// <summary>
/// Periodically removes expired games from the in-memory store so abandoned and
/// finished games do not accumulate for the lifetime of the process.
/// </summary>
public sealed class GameCleanupService : BackgroundService
{
    private readonly GameEngine _engine;
    private readonly GameCleanupOptions _options;
    private readonly ILogger<GameCleanupService> _logger;

    public GameCleanupService(
        GameEngine engine,
        IOptions<GameCleanupOptions> options,
        ILogger<GameCleanupService> logger)
    {
        _engine = engine;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.SweepInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                var removed = _engine.PurgeExpiredGames(
                    _options.CompletedGameTtl,
                    _options.IdleGameTtl);

                if (removed > 0)
                {
                    _logger.LogInformation("Removed {Count} expired game(s).", removed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Game cleanup sweep failed.");
            }
        }
    }
}
