using Battleship.Core.Models;
using Battleship.Core.Validation;

namespace Battleship.Core.Engine;

/// <summary>
/// Applies a single shot to a target board and reports the result. All move
/// validation (range, already-played) lives here so callers pass a location and
/// handle the resulting <see cref="MoveOutcome"/>.
/// </summary>
public sealed class MoveProcessor
{
    /// <summary>
    /// Applies a shot at <paramref name="location"/> against <paramref name="targetBoard"/>,
    /// mutating its hit/miss state. Returns the shot result, or a validation failure
    /// (the board is left unchanged on failure).
    /// </summary>
    public MoveOutcome ApplyShot(Board targetBoard, int location)
    {
        ArgumentNullException.ThrowIfNull(targetBoard);

        if (location < BoardValidationConstants.MinLocation || location > BoardValidationConstants.MaxLocation)
        {
            return MoveOutcome.Failure(
                location,
                ValidationErrorCode.InvalidLocation,
                $"Location must be between {BoardValidationConstants.MinLocation} and {BoardValidationConstants.MaxLocation}.");
        }

        var shot = BoardBits.LocationMask(location);

        if (((targetBoard.Hits | targetBoard.Misses) & shot) != 0)
        {
            return MoveOutcome.Failure(
                location,
                ValidationErrorCode.LocationAlreadyPlayed,
                $"Location {location} has already been played.");
        }

        if ((targetBoard.Occupancy & shot) == 0)
        {
            targetBoard.Misses |= shot;
            return MoveOutcome.Success(location, ShotResult.Miss, null);
        }

        targetBoard.Hits |= shot;

        ShipType? sunkShip = null;
        foreach (var ship in targetBoard.Ships)
        {
            if ((ship.Positions & shot) != 0)
            {
                if (targetBoard.IsSunk(ship))
                {
                    sunkShip = ship.Type;
                }

                break;
            }
        }

        if (AllShipsSunk(targetBoard))
        {
            return MoveOutcome.Success(location, ShotResult.Win, sunkShip);
        }

        return sunkShip is not null
            ? MoveOutcome.Success(location, ShotResult.Sunk, sunkShip)
            : MoveOutcome.Success(location, ShotResult.Hit, null);
    }

    private static bool AllShipsSunk(Board board)
    {
        foreach (var ship in board.Ships)
        {
            if (!board.IsSunk(ship))
            {
                return false;
            }
        }

        return true;
    }
}
