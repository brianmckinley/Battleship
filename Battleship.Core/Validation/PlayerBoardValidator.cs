using Battleship.Core.Models;
using Battleship.Core.Placement;

namespace Battleship.Core.Validation;

/// <summary>
/// Validates a submitted 100-character player board and, on success, builds the
/// authoritative <see cref="Board"/> with ships in canonical order.
///
/// All create-game validation and fallback logic lives here so callers only
/// need to pass the raw board string and handle the result.
/// </summary>
public sealed class PlayerBoardValidator
{
    private readonly ValidPlacements _validPlacements;

    private static readonly IReadOnlyDictionary<char, ShipType> ShipCharToType =
        ShipDefinition.Catalog.ToDictionary(s => s.PlacementCharacter, s => s.Type);

    public PlayerBoardValidator(ValidPlacements validPlacements)
    {
        _validPlacements = validPlacements;
    }

    public BoardValidationResult Validate(string? playerBoard)
    {
        if (playerBoard is null || playerBoard.Length != BoardValidationConstants.BoardStringLength)
        {
            return BoardValidationResult.Failure(
                ValidationErrorCode.InvalidBoardLength,
                $"Board must contain exactly {BoardValidationConstants.BoardStringLength} characters.");
        }

        var masks = new Dictionary<ShipType, UInt128>();
        var counts = new Dictionary<ShipType, int>();

        for (var index = 0; index < playerBoard.Length; index++)
        {
            var character = playerBoard[index];

            if (character == BoardValidationConstants.EmptyWater)
            {
                continue;
            }

            if (!BoardValidationConstants.AllowedPlacementCharacters.Contains(character))
            {
                return BoardValidationResult.Failure(
                    ValidationErrorCode.InvalidBoard,
                    $"Board contains disallowed character '{character}' at index {index}.");
            }

            var type = ShipCharToType[character];
            masks[type] = masks.GetValueOrDefault(type) | BoardBits.LocationMask(index);
            counts[type] = counts.GetValueOrDefault(type) + 1;
        }

        var ships = new List<Ship>(ShipDefinition.OrderedTypes.Count);

        foreach (var type in ShipDefinition.OrderedTypes)
        {
            var definition = ShipDefinition.For(type);
            var squareCount = counts.GetValueOrDefault(type);

            if (squareCount != definition.Length)
            {
                return BoardValidationResult.Failure(
                    ValidationErrorCode.InvalidShipCount,
                    $"{type} must occupy exactly {definition.Length} squares but occupied {squareCount}.");
            }

            var mask = masks.GetValueOrDefault(type);

            if (!_validPlacements.IsValidPlacement(type, mask))
            {
                return BoardValidationResult.Failure(
                    ValidationErrorCode.InvalidShipPlacement,
                    $"{type} placement is not a valid horizontal or vertical line.");
            }

            ships.Add(Ship.Create(type, mask));
        }

        var board = new Board { Ships = ships.ToArray() };
        return BoardValidationResult.Success(board);
    }
}
