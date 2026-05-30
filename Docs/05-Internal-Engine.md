## 14. Internal Engine Design

Internally the game uses bitmaps rather than coordinate collections.

The implementation uses .NET’s built-in `UInt128`.

```text
Bits 0-99     Board positions
Bits 100-127  Metadata
```

No custom bitmap wrapper is required.

Use `UInt128` directly for all board operations.

---

## 15. Board Constants

```csharp
public static class BoardBits
{
    public const int BoardSize = 100;
    public const int ShipTypeShift = 100;

    public static readonly UInt128 PositionMask =
        (((UInt128)1 << BoardSize) - 1);

    public static UInt128 LocationMask(int location)
    {
        return (UInt128)1 << location;
    }
}
```

If a compiler/runtime issue appears with shifting by 100, implement `PositionMask` by setting bits 0-99 in a loop.

---

## 16. Ship Types

```csharp
public enum ShipType
{
    Carrier = 1,
    Battleship = 2,
    Destroyer = 3,
    Submarine = 4,
    PatrolBoat = 5
}
```

Ships are always stored in this order:

1. Carrier
2. Battleship
3. Destroyer
4. Submarine
5. Patrol Boat

---

## 17. Ship Structure

Each ship is represented by a single `UInt128` value.

```csharp
public struct Ship
{
    public UInt128 Data { get; set; }

    public UInt128 Positions
    {
        get
        {
            return Data & BoardBits.PositionMask;
        }
    }

    public ShipType Type
    {
        get
        {
            return (ShipType)((Data >> BoardBits.ShipTypeShift) & 0b111);
        }
    }
}
```

Layout:

```text
0-99      Position bitmap
100-102   Ship Type
103-127   Reserved
```

---

## 18. Board Structure

```csharp
public class Board
{
    public Ship[] Ships { get; init; } = [];

    public UInt128 Hits { get; set; }

    public UInt128 Misses { get; set; }
}
```

`Hits` contains locations where successful shots occurred.

`Misses` contains locations where missed shots occurred.

---

## 19. Game Structure

```csharp
public class Game
{
    public Guid GameId { get; init; }

    public Board PlayerBoard { get; init; } = new();

    public Board BotBoard { get; init; } = new();

    public GameStatus Status { get; set; }

    public MoveResult? LastPlayerMove { get; set; }

    public MoveResult? LastBotMove { get; set; }
}
```

---

## 20. Game Storage

Use in-memory storage:

```csharp
ConcurrentDictionary<Guid, Game>
```

Do not create:

- Database persistence
- Repository abstraction unless truly needed
- User/session storage

---

## 21. Hit Detection

Create a bitmap for the selected location:

```csharp
UInt128 shot = BoardBits.LocationMask(location);
```

Determine whether any ship occupies that location:

```csharp
(ship.Positions & shot) != 0
```

If hit:

```csharp
board.Hits |= shot;
```

If miss:

```csharp
board.Misses |= shot;
```

---

## 22. Ship Sunk Detection

A ship is sunk when all occupied locations have been hit.

```csharp
(ship.Positions & board.Hits) == ship.Positions
```

---

## 23. Board Occupancy

Board occupancy is generated from ship bitmaps.

```csharp
UInt128 occupancy = 0;

foreach (var ship in board.Ships)
{
    occupancy |= ship.Positions;
}
```

Used for:

- Hit detection
- Bot placement
- Serialization
- Validation

---

## 24. Internal Serialization Rules

Board strings are generated from internal bitmap structures.

Board strings are never treated as the source of truth after game creation.

Board strings must not be stored in the `Game` object.

The authoritative state is:

```text
Ships
Hits
Misses
Status
LastPlayerMove
LastBotMove
```

### Player Board Generation

Generated from:

```text
Player Ships
Bot Hits
Bot Misses
```

Output characters:

```text
.
o
C B D S P
c b d s p
```

### Opponent Board Generation

Generated from:

```text
Bot Ships
Player Hits
Player Misses
```

Output characters:

```text
.
o
x
C B D S P
```

Opponent board rule:

- Show `x` for hits on unsunk ships.
- Show the ship character `C/B/D/S/P` for all segments of sunk ships.
- Show `o` for misses.
- Show `.` for unselected squares.

---

