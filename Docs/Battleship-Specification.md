# Battleship Design Specification

## 1. Overview

This project implements a simple Battleship game where a human player competes against a server-controlled opponent.

Design goals:

- Simple API
- Server-authoritative game state
- Stateless client rendering
- Compact game state representation
- Efficient server-side implementation
- In-memory game storage
- SVG-based user interface
- No user accounts
- No multiplayer support
- No game history
- No matchmaking
- No authentication
- No database
- No SignalR

The server maintains game state in memory using a `Guid` Game ID.

The client never calculates:

- Hits
- Misses
- Sunk ships
- Game status

The client only renders the game state returned by the API.

---

## 2. Technology Choices

### Backend

- ASP.NET Core 9
- Minimal API
- Swagger/OpenAPI
- C#
- In-memory storage only

### Frontend

- React
- TypeScript
- Vite
- SVG rendering
- Plain CSS or CSS Modules
- No UI framework required

### Tests

Create unit tests for the core game engine.

---

## 3. Board Coordinates

The board is a standard 10×10 grid.

Board positions are represented as indexes from 0 to 99.

Mapping:

| Index | Position |
|---:|---|
| 0 | A1 |
| 1 | A2 |
| 2 | A3 |
| ... | ... |
| 9 | A10 |
| 10 | B1 |
| ... | ... |
| 99 | J10 |

All API move requests use board indexes from 0 to 99.

---

## 4. Board String Encoding

The API exposes each board as a fixed-length 100-character string.

The string index maps directly to the board index.

### Opponent Board Encoding

The opponent board represents what the player knows about the enemy fleet.

| Character | Meaning |
|---|---|
| `.` | Unselected / available target |
| `o` | Player fired here and missed |
| `x` | Player hit a ship that has not been sunk yet |
| `C` | Carrier square revealed after sinking |
| `B` | Battleship square revealed after sinking |
| `D` | Destroyer square revealed after sinking |
| `S` | Submarine square revealed after sinking |
| `P` | Patrol Boat square revealed after sinking |

When a ship is sunk, the server replaces the corresponding `x` markers with the ship identifier so the entire ship becomes visible.

Example conceptual board:

```text
....o.....
....xxx...
..........
..DDD.....
..........
..........
..........
..........
..........
..........
```

### Player Board Encoding

The player board contains the player’s ships and the bot’s attacks.

| Character | Meaning |
|---|---|
| `.` | Empty water |
| `o` | Bot fired here and missed |
| `C` | Carrier segment |
| `B` | Battleship segment |
| `D` | Destroyer segment |
| `S` | Submarine segment |
| `P` | Patrol Boat segment |
| `c` | Carrier segment hit |
| `b` | Battleship segment hit |
| `d` | Destroyer segment hit |
| `s` | Submarine segment hit |
| `p` | Patrol Boat segment hit |

Example conceptual board:

```text
CCCCc.....
..........
..oo......
BBBb......
..........
..........
..........
..........
..........
..........
```

---

## 5. Game Status Values

| Value | Meaning |
|---|---|
| `in_progress` | Game is active |
| `player_won` | Player has sunk all bot ships |
| `bot_won` | Bot has sunk all player ships |

There is no `turn` field.

Every returned game state is implicitly waiting for the player unless the game is over.

The bot always moves immediately after the player submits a move.

---

## 6. Move Result Values

| Value | Meaning |
|---|---|
| `miss` | Shot missed |
| `hit` | Shot hit a ship |
| `sunk` | Shot sunk a ship |
| `win` | Shot won the game |

---

## 7. DTO Definitions

### CreateGameRequest

```csharp
public class CreateGameRequest
{
    public string PlayerBoard { get; set; } = "";

    public bool BotMovesFirst { get; set; }
}
```

### MoveRequest

```csharp
public class MoveRequest
{
    public int Location { get; set; }
}
```

`Location` must be between 0 and 99.

### MoveResult

```csharp
public class MoveResult
{
    public int Location { get; set; }

    public string Result { get; set; } = "";
}
```

### GameState

```csharp
public class GameState
{
    public string GameId { get; set; } = "";

    public string Status { get; set; } = "";

    public string PlayerBoard { get; set; } = "";

    public string OpponentBoard { get; set; } = "";

    public MoveResult? LastPlayerMove { get; set; }

    public MoveResult? LastBotMove { get; set; }
}
```

### ErrorResponse

```csharp
public class ErrorResponse
{
    public string Message { get; set; } = "";

    public string Code { get; set; } = "";
}
```

---

## 8. API Contract

### Create Game

Creates a new game and submits the player's ship placement.

If `botMovesFirst` is true, the server executes the bot’s opening move before returning.

Request:

```http
POST /games
```

Request body:

```json
{
  "playerBoard": "CCCCC.....BBBB......DDD.......SSS.......PP.........................................................",
  "botMovesFirst": true
}
```

Response:

```json
{
  "gameId": "11111111-1111-1111-1111-111111111111",
  "status": "in_progress",
  "playerBoard": "CCCCC.....BBBB......DDD.......SSS.......PP......................o..................................",
  "opponentBoard": "....................................................................................................",
  "lastPlayerMove": null,
  "lastBotMove": {
    "location": 62,
    "result": "miss"
  }
}
```

If `botMovesFirst` is false, `lastBotMove` is null and the returned player board has no bot shot applied.

---

### Submit Move

Processes the player's move and then immediately processes the bot's move unless the player's move wins the game.

Request:

```http
POST /games/{gameId}/moves
```

Request body:

```json
{
  "location": 34
}
```

Response:

```json
{
  "gameId": "11111111-1111-1111-1111-111111111111",
  "status": "in_progress",
  "playerBoard": "CCCCc..............oo.....BBBb.....................................................................",
  "opponentBoard": "....o......xxx.................DDD.................................................................",
  "lastPlayerMove": {
    "location": 34,
    "result": "hit"
  },
  "lastBotMove": {
    "location": 18,
    "result": "miss"
  }
}
```

Rules:

- If the player wins with the submitted move, the bot does not move afterward.
- If the game is already complete, return an error.
- If the selected opponent location was already played, return an error.

---

### Get Game State

Returns the current game state.

Request:

```http
GET /games/{gameId}
```

Response:

```json
{
  "gameId": "11111111-1111-1111-1111-111111111111",
  "status": "in_progress",
  "playerBoard": "...",
  "opponentBoard": "...",
  "lastPlayerMove": {
    "location": 34,
    "result": "hit"
  },
  "lastBotMove": {
    "location": 18,
    "result": "miss"
  }
}
```

---

### Delete Game

Deletes a game from memory.

Request:

```http
DELETE /games/{gameId}
```

Response:

```json
{
  "success": true
}
```

---

## 9. API Error Responses

Standard error response:

```json
{
  "message": "Invalid ship placement",
  "code": "InvalidShipPlacement"
}
```

Error codes:

| Code | Meaning |
|---|---|
| `InvalidBoard` | Board string invalid |
| `InvalidBoardLength` | Board must contain 100 characters |
| `InvalidShipCount` | Incorrect ship count |
| `InvalidShipPlacement` | Invalid ship shape |
| `InvalidLocation` | Location must be 0-99 |
| `LocationAlreadyPlayed` | Location already selected |
| `GameNotFound` | Unknown game id |
| `GameOver` | Game already completed |

Suggested HTTP statuses:

| Scenario | Status |
|---|---:|
| Invalid request or validation failure | 400 |
| Game not found | 404 |
| Game already over | 409 |
| Location already played | 409 |

---

## 10. Create Game Validation

The supplied `PlayerBoard` must:

- Be exactly 100 characters long
- Contain exactly one Carrier
- Contain exactly one Battleship
- Contain exactly one Destroyer
- Contain exactly one Submarine
- Contain exactly one Patrol Boat
- Contain no damage markers
- Contain no miss markers

Allowed create-game characters:

```text
.
C
B
D
S
P
```

Disallowed create-game characters:

```text
o
x
c
b
d
s
p
```

Ship requirements:

| Ship | Character | Length | Count |
|---|---|---:|---:|
| Carrier | `C` | 5 | 1 |
| Battleship | `B` | 4 | 1 |
| Destroyer | `D` | 3 | 1 |
| Submarine | `S` | 3 | 1 |
| Patrol Boat | `P` | 2 | 1 |

Total ship squares:

```text
17
```

---

## 11. Placement Mask Generation

At application startup, the server generates all valid ship placement masks.

Placement masks must be generated programmatically.

Placement masks must not be hard-coded.

Each mask is represented by a `UInt128`.

Only the first 100 bits are used for board positions.

Example length 3 horizontal placement at row 0, column 2:

```text
..XXX.....
..........
..........
..........
..........
..........
..........
..........
..........
..........
```

Conceptual bitmap:

```csharp
((UInt128)1 << 2) |
((UInt128)1 << 3) |
((UInt128)1 << 4)
```

Valid placement counts:

| Ship Length | Horizontal | Vertical | Total |
|---:|---:|---:|---:|
| 5 | 60 | 60 | 120 |
| 4 | 70 | 70 | 140 |
| 3 | 80 | 80 | 160 |
| 2 | 90 | 90 | 180 |

Formula:

```text
horizontal count = 10 * (11 - length)
vertical count   = 10 * (11 - length)
```

---

## 12. Placement Collections

The server maintains both a lookup collection and a placement list.

For validation:

```csharp
Dictionary<ShipType, HashSet<UInt128>>
```

For bot placement:

```csharp
Dictionary<ShipType, UInt128[]>
```

Example:

```csharp
ValidPlacements[ShipType.Carrier]
```

contains every valid carrier placement.

---

## 13. Ship Extraction and Validation

During game creation, the server extracts a bitmap for each ship type from the submitted `PlayerBoard`.

Example extracted masks:

```csharp
UInt128 carrierMask;
UInt128 battleshipMask;
UInt128 destroyerMask;
UInt128 submarineMask;
UInt128 patrolBoatMask;
```

Each bitmap contains all board positions occupied by that ship.

Ship validation is performed solely by checking whether the extracted bitmap exists in the precomputed placement collection.

Example:

```csharp
ValidPlacements[ShipType.Carrier].Contains(carrierMask);
```

This automatically validates:

- Ship length
- Horizontal placement
- Vertical placement
- Contiguous squares
- No diagonal placement
- No row wrapping

No additional geometric validation is required.

Remove separate validation logic for contiguity, direction, diagonal placement, and row wrapping. Those conditions are enforced by placement mask membership.

---

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

## 27. UI Design

### Setup Screen

Displays:

- 10×10 board
- Ship palette
- Drag-and-drop ship placement
- Rotate button
- Start Game button

Requirements:

- Prevent overlap
- Prevent out-of-bounds placement
- Prevent invalid rotations
- Disable Start Game until all ships are placed
- Generate the 100-character `playerBoard` string for `POST /games`

### Game Screen

Displays:

- Player board
- Opponent board
- Last player move
- Last bot move

Player board shows:

- Ships
- Ship damage
- Bot misses

Opponent board shows:

- Available targets
- Hits
- Misses
- Revealed sunk ships

### Game Over Screen

Victory message:

```text
You Win
```

Defeat message:

```text
You Lose
```

Actions:

```text
New Game
```

---

## 28. SVG Asset Strategy

Ships are rendered using SVG assets.

The UI uses SVG images to render ship segments on the board.

The game engine remains bitmap-based internally.

The API board strings remain the single source of truth for UI rendering.

### SVG Files

Common components:

```text
bow.svg
stern.svg
```

Ship-specific middle components:

```text
carrier-middle.svg
battleship-middle.svg
destroyer-middle.svg
submarine-middle.svg
patrolboat-middle.svg
```

### Ship Assembly

Carrier:

```text
[Bow][Carrier Mid][Carrier Mid][Carrier Mid][Stern]
```

Battleship:

```text
[Bow][Battleship Mid][Battleship Mid][Stern]
```

Destroyer:

```text
[Bow][Destroyer Mid][Stern]
```

Submarine:

```text
[Bow][Submarine Mid][Stern]
```

Patrol Boat:

```text
[Bow][Stern]
```

### Ship Rotation

Ships are authored horizontally.

Vertical ships use CSS rotation or SVG transform.

No separate vertical SVG files are required.

Example:

```css
.ship-vertical
{
    transform: rotate(90deg);
}
```

### Ship Damage Display

Damage is determined from board encoding.

Examples:

```text
C = Carrier segment
c = Carrier segment hit
B = Battleship segment
b = Battleship segment hit
```

Damage is rendered by overlaying a red X.

No separate hit SVG assets are required.

---

## 29. Unit Tests

Create tests for:

- Placement mask generation
- Valid placement counts
- Board validation
- Ship extraction
- Invalid ship shape rejection
- Hit detection
- Miss detection
- Sunk detection
- Player board serialization
- Opponent board serialization
- Bot ship placement
- Bot never repeats a move
- Player win detection
- Bot win detection

---

## 30. Project Structure

```text
Battleship.sln

Battleship.Api
    Program.cs
    DTOs
    Swagger

Battleship.Core
    Models
    GameEngine
    AI
    Validation
    Serialization
    Placement

Battleship.Tests
    UnitTests

battleship-ui
    src
    public
        svg
```

---

## 31. Cursor Implementation Notes

Generate:

1. ASP.NET Core 9 Minimal API
2. Swagger/OpenAPI
3. React + TypeScript + Vite UI
4. SVG board rendering
5. In-memory game storage
6. UInt128 bitmap-based game engine
7. Placement-mask validation
8. Random bot placement
9. Random bot targeting
10. Unit tests for core game logic

Do not add:

- Authentication
- Database persistence
- SignalR
- Multiplayer
- Matchmaking
- User accounts
- Chat
- External services
- UI framework unless explicitly requested

Keep the implementation simple and prototype-oriented.

The server is authoritative.

The client renders only the state returned by the API.
