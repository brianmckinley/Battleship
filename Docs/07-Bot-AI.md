## 25. Bot Ship Placement

Bot ship placement uses the same placement collections as validation.

Process:

```text
occupied = 0

For each ship, in ship order:
    get all valid placements for that ship type
    filter placements where (mask & occupied) == 0
    randomly select one placement
    create ship with selected mask and ship type metadata
    occupied |= selected mask
```

Conceptually:

```csharp
var availablePlacements =
    placementList[type]
        .Where(mask => (mask & occupied) == 0)
        .ToArray();

var selectedPlacement =
    availablePlacements[Random.Shared.Next(availablePlacements.Length)];

occupied |= selectedPlacement;
```

Use `Random.Shared` for MVP randomness.

---

## 26. Bot Targeting AI

Initial implementation:

```text
Random AI
```

Rules:

- Select randomly from available player-board locations.
- Never repeat a move.
- No hunt/target strategy.
- Use `Random.Shared`.

Available target locations are those not in:

```text
PlayerBoard.Hits | PlayerBoard.Misses
```

Future AI improvements must not require API changes.

---

