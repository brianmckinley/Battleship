using Battleship.Core;
using Battleship.Core.AI;
using Battleship.Core.Placement;
using Battleship.Core.Serialization;
using Battleship.Core.Validation;

namespace Battleship.Tests;

public class BotPlacementServiceTests
{
    private readonly ValidPlacements _validPlacements = new();
    private readonly PlayerBoardValidator _validator;

    public BotPlacementServiceTests()
    {
        _validator = new PlayerBoardValidator(_validPlacements);
    }

    [Fact]
    public void CreateBoard_PlacesAllFiveShips()
    {
        var bot = new BotPlacementService(_validPlacements, new Random(1));

        var board = bot.CreateBoard();

        Assert.Equal(5, board.Ships.Length);
    }

    [Fact]
    public void CreateBoard_ProducesExactly17NonOverlappingSquares()
    {
        var bot = new BotPlacementService(_validPlacements, new Random(2));

        var board = bot.CreateBoard();

        var occupied = 0;
        for (var i = 0; i < BoardBits.BoardSize; i++)
        {
            if ((board.Occupancy & BoardBits.LocationMask(i)) != 0)
            {
                occupied++;
            }
        }

        Assert.Equal(17, occupied);
    }

    [Fact]
    public void CreateBoard_AlwaysProducesAValidFleet()
    {
        // Exercise many seeds; every generated board must pass create-game validation.
        for (var seed = 0; seed < 500; seed++)
        {
            var bot = new BotPlacementService(_validPlacements, new Random(seed));
            var board = bot.CreateBoard();

            var serialized = BoardSerializer.SerializePlayerView(board);
            var result = _validator.Validate(serialized);

            Assert.True(result.IsValid, $"Seed {seed} produced an invalid fleet: {result.ErrorCode}");
        }
    }

    [Fact]
    public void CreateBoard_IsDeterministicForSameSeed()
    {
        var first = new BotPlacementService(_validPlacements, new Random(42)).CreateBoard();
        var second = new BotPlacementService(_validPlacements, new Random(42)).CreateBoard();

        Assert.Equal(first.Occupancy, second.Occupancy);
    }
}
