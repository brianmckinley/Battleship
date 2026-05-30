using Battleship.Core.Engine;
using Battleship.Core.Models;

namespace Battleship.Tests;

public class GameStoreTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static readonly TimeSpan CompletedTtl = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan IdleTtl = TimeSpan.FromHours(3);

    private static Game NewGame(GameStatus status, DateTimeOffset lastActivity) => new()
    {
        GameId = Guid.NewGuid(),
        Status = status,
        CreatedAt = lastActivity,
        LastActivityAt = lastActivity,
    };

    [Fact]
    public void RemoveExpired_RemovesFinishedGamesPastCompletedTtl()
    {
        var store = new GameStore();
        var stale = NewGame(GameStatus.PlayerWon, Now - TimeSpan.FromMinutes(31));
        var fresh = NewGame(GameStatus.PlayerWon, Now - TimeSpan.FromMinutes(10));
        store.Add(stale);
        store.Add(fresh);

        var removed = store.RemoveExpired(Now, CompletedTtl, IdleTtl);

        Assert.Equal(1, removed);
        Assert.Null(store.Get(stale.GameId));
        Assert.NotNull(store.Get(fresh.GameId));
    }

    [Fact]
    public void RemoveExpired_RemovesAbandonedInProgressGamesPastIdleTtl()
    {
        var store = new GameStore();
        var abandoned = NewGame(GameStatus.InProgress, Now - TimeSpan.FromHours(4));
        var active = NewGame(GameStatus.InProgress, Now - TimeSpan.FromMinutes(45));
        store.Add(abandoned);
        store.Add(active);

        var removed = store.RemoveExpired(Now, CompletedTtl, IdleTtl);

        Assert.Equal(1, removed);
        Assert.Null(store.Get(abandoned.GameId));
        Assert.NotNull(store.Get(active.GameId));
    }

    [Fact]
    public void RemoveExpired_KeepsInProgressGameThatExceedsCompletedTtlButNotIdleTtl()
    {
        var store = new GameStore();
        // 45 min idle: past the completed TTL, but in-progress games use the longer idle TTL.
        var game = NewGame(GameStatus.InProgress, Now - TimeSpan.FromMinutes(45));
        store.Add(game);

        var removed = store.RemoveExpired(Now, CompletedTtl, IdleTtl);

        Assert.Equal(0, removed);
        Assert.NotNull(store.Get(game.GameId));
    }
}
