using System.Diagnostics.CodeAnalysis;
using Battleship.Core.Models;
using Battleship.Core.Placement;
using Battleship.Core.Validation;

namespace Battleship.Core.AI;

/// <summary>
/// Places the bot fleet by randomly selecting from the precomputed valid
/// placement masks, skipping any that overlap already-placed ships.
/// Randomness is injectable for deterministic tests.
/// </summary>
public sealed class BotPlacementService
{
    private const int MaxFleetAttempts = 100;

    private readonly ValidPlacements _validPlacements;
    private readonly Random _random;

    public BotPlacementService(ValidPlacements validPlacements, Random? random = null)
    {
        _validPlacements = validPlacements;
        _random = random ?? Random.Shared;
    }

    /// <summary>
    /// Builds a fully placed, non-overlapping bot board. Retries the whole fleet
    /// if random selection reaches a dead end with no room for a later ship.
    /// </summary>
    public Board CreateBoard()
    {
        for (var attempt = 0; attempt < MaxFleetAttempts; attempt++)
        {
            if (TryPlaceFleet(out var board))
            {
                return board;
            }
        }

        throw new InvalidOperationException(
            $"Unable to place the bot fleet within {MaxFleetAttempts} attempts.");
    }

    private bool TryPlaceFleet([NotNullWhen(true)] out Board? board)
    {
        UInt128 occupied = 0;
        var ships = new Ship[ShipDefinition.OrderedTypes.Count];

        for (var i = 0; i < ShipDefinition.OrderedTypes.Count; i++)
        {
            var type = ShipDefinition.OrderedTypes[i];
            var candidates = _validPlacements.Arrays[type];

            var available = new List<UInt128>(candidates.Length);
            foreach (var mask in candidates)
            {
                if ((mask & occupied) == 0)
                {
                    available.Add(mask);
                }
            }

            if (available.Count == 0)
            {
                board = null;
                return false;
            }

            var selected = available[_random.Next(available.Count)];
            ships[i] = Ship.Create(type, selected);
            occupied |= selected;
        }

        board = new Board { Ships = ships };
        return true;
    }
}
