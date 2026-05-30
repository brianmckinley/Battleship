using Battleship.Core;
using Battleship.Core.AI;
using Battleship.Core.Engine;
using Battleship.Core.Models;
using Battleship.Core.Placement;
using Battleship.Core.Validation;

namespace Battleship.Tests;

public class GameEngineTests
{
    private static GameEngine NewEngine(int placementSeed = 1, int targetingSeed = 2)
    {
        var validPlacements = new ValidPlacements();
        return new GameEngine(
            new PlayerBoardValidator(validPlacements),
            new BotPlacementService(validPlacements, new Random(placementSeed)),
            new BotTargetingService(new Random(targetingSeed)),
            new MoveProcessor(),
            new GameStore(),
            TimeProvider.System);
    }

    [Fact]
    public void CreateGame_ValidBoard_StartsInProgress()
    {
        var engine = NewEngine();

        var result = engine.CreateGame(TestBoards.ValidFleet(), botMovesFirst: false);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Game);
        Assert.Equal(GameStatus.InProgress, result.Game!.Status);
        Assert.Null(result.Game.LastPlayerMove);
        Assert.Null(result.Game.LastBotMove);
        Assert.NotEqual(Guid.Empty, result.Game.GameId);
    }

    [Fact]
    public void CreateGame_InvalidBoard_FailsWithValidationCode()
    {
        var engine = NewEngine();

        var result = engine.CreateGame("too-short", botMovesFirst: false);

        Assert.False(result.IsSuccess);
        Assert.Equal(ValidationErrorCode.InvalidBoardLength, result.ErrorCode);
        Assert.Null(result.Game);
    }

    [Fact]
    public void CreateGame_BotMovesFirst_RecordsBotMove()
    {
        var engine = NewEngine();

        var result = engine.CreateGame(TestBoards.ValidFleet(), botMovesFirst: true);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Game!.LastBotMove);
        Assert.Null(result.Game.LastPlayerMove);
    }

    [Fact]
    public void CreateGame_StoresRetrievableGame()
    {
        var engine = NewEngine();
        var created = engine.CreateGame(TestBoards.ValidFleet(), botMovesFirst: false);

        var fetched = engine.GetGame(created.Game!.GameId);

        Assert.True(fetched.IsSuccess);
        Assert.Same(created.Game, fetched.Game);
    }

    [Fact]
    public void SubmitMove_UnknownGame_ReturnsGameNotFound()
    {
        var engine = NewEngine();

        var result = engine.SubmitMove(Guid.NewGuid(), 0);

        Assert.False(result.IsSuccess);
        Assert.Equal(ValidationErrorCode.GameNotFound, result.ErrorCode);
    }

    [Fact]
    public void SubmitMove_RecordsPlayerAndBotMoves()
    {
        var engine = NewEngine();
        var game = engine.CreateGame(TestBoards.ValidFleet(), botMovesFirst: false).Game!;

        var result = engine.SubmitMove(game.GameId, 5); // empty water on bot board is fine either way

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Game!.LastPlayerMove);
        Assert.Equal(5, result.Game.LastPlayerMove!.Location);
        Assert.NotNull(result.Game.LastBotMove); // bot replied because player did not win
    }

    [Fact]
    public void SubmitMove_RepeatedLocation_Fails()
    {
        var engine = NewEngine();
        var game = engine.CreateGame(TestBoards.ValidFleet(), botMovesFirst: false).Game!;

        engine.SubmitMove(game.GameId, 7);
        var result = engine.SubmitMove(game.GameId, 7);

        Assert.False(result.IsSuccess);
        Assert.Equal(ValidationErrorCode.LocationAlreadyPlayed, result.ErrorCode);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100)]
    public void SubmitMove_OutOfRange_Fails(int location)
    {
        var engine = NewEngine();
        var game = engine.CreateGame(TestBoards.ValidFleet(), botMovesFirst: false).Game!;

        var result = engine.SubmitMove(game.GameId, location);

        Assert.False(result.IsSuccess);
        Assert.Equal(ValidationErrorCode.InvalidLocation, result.ErrorCode);
    }

    [Fact]
    public void SubmitMove_SinkingAllBotShips_WinsGame()
    {
        var engine = NewEngine();
        var game = engine.CreateGame(TestBoards.ValidFleet(), botMovesFirst: false).Game!;

        var botTargets = OccupiedLocations(game.BotBoard);
        GameOperationResult? result = null;

        foreach (var location in botTargets)
        {
            result = engine.SubmitMove(game.GameId, location);
        }

        Assert.NotNull(result);
        Assert.True(result!.IsSuccess);
        Assert.Equal(GameStatus.PlayerWon, result.Game!.Status);
        Assert.Equal(MoveResultValues.Win, result.Game.LastPlayerMove!.Result);
    }

    [Fact]
    public void SubmitMove_AfterGameOver_ReturnsGameOver()
    {
        var engine = NewEngine();
        var game = engine.CreateGame(TestBoards.ValidFleet(), botMovesFirst: false).Game!;

        foreach (var location in OccupiedLocations(game.BotBoard))
        {
            engine.SubmitMove(game.GameId, location);
        }

        // Pick an unplayed empty-water square to attempt another move.
        var result = engine.SubmitMove(game.GameId, FirstUnplayed(game.BotBoard));

        Assert.False(result.IsSuccess);
        Assert.Equal(ValidationErrorCode.GameOver, result.ErrorCode);
    }

    [Fact]
    public void DeleteGame_RemovesGame()
    {
        var engine = NewEngine();
        var game = engine.CreateGame(TestBoards.ValidFleet(), botMovesFirst: false).Game!;

        Assert.True(engine.DeleteGame(game.GameId));
        Assert.False(engine.GetGame(game.GameId).IsSuccess);
        Assert.False(engine.DeleteGame(game.GameId)); // already gone
    }

    private static int[] OccupiedLocations(Board board)
    {
        var locations = new List<int>();
        for (var i = 0; i < BoardBits.BoardSize; i++)
        {
            if ((board.Occupancy & BoardBits.LocationMask(i)) != 0)
            {
                locations.Add(i);
            }
        }

        return locations.ToArray();
    }

    private static int FirstUnplayed(Board board)
    {
        var played = board.Hits | board.Misses;
        for (var i = 0; i < BoardBits.BoardSize; i++)
        {
            if ((played & BoardBits.LocationMask(i)) == 0)
            {
                return i;
            }
        }

        return 0;
    }
}
