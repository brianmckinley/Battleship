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

