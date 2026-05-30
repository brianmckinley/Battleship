using System.Collections.Concurrent;
using Battleship.Core.Models;

namespace Battleship.Core.Engine;

/// <summary>
/// In-memory game storage keyed by game id. No persistence by design (spec §20).
/// </summary>
public sealed class GameStore
{
    private readonly ConcurrentDictionary<Guid, Game> _games = new();

    public void Add(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        _games[game.GameId] = game;
    }

    public Game? Get(Guid gameId) =>
        _games.TryGetValue(gameId, out var game) ? game : null;

    public bool Remove(Guid gameId) =>
        _games.TryRemove(gameId, out _);

    /// <summary>
    /// Returns up to <paramref name="max"/> stored games. Ordering is not
    /// guaranteed because the underlying store is unordered.
    /// </summary>
    public IReadOnlyList<Game> List(int max)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(max);

        var games = new List<Game>(Math.Min(max, _games.Count));
        foreach (var game in _games.Values)
        {
            if (games.Count >= max)
            {
                break;
            }

            games.Add(game);
        }

        return games;
    }
}
