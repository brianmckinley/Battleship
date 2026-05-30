# Battleship

A full-stack implementation of the classic **Battleship** game: a .NET 8 REST API that runs the game engine and bot opponent, plus a React + TypeScript web client for playing in the browser.

The player deploys a fleet on a 10×10 grid and trades shots with a computer-controlled bot until one side's entire fleet is sunk.

---

## Contents

- [Architecture](#architecture)
- [Project layout](#project-layout)
- [Game rules](#game-rules)
- [Board encoding](#board-encoding)
- [Getting started](#getting-started)
- [API reference](#api-reference)
- [Running the tests](#running-the-tests)
- [Design notes](#design-notes)
- [Specification](#specification)

---

## Architecture

```
┌─────────────────────┐        HTTP/JSON        ┌──────────────────────────┐
│   battleship-ui     │  ───────────────────▶   │      Battleship.Api       │
│  (React + Vite TS)  │   POST /game, /moves    │   (ASP.NET Minimal API)   │
│                     │  ◀───────────────────   │                           │
└─────────────────────┘     game state JSON     └────────────┬─────────────┘
                                                              │
                                                              ▼
                                                 ┌──────────────────────────┐
                                                 │      Battleship.Core      │
                                                 │  engine · validation ·    │
                                                 │  placement · bot AI       │
                                                 └──────────────────────────┘
```

- **`Battleship.Core`** — the engine. Board state is held as `UInt128` bitmaps; this project owns ship placement, validation, move processing, the in-memory game store, and the bot's placement/targeting logic. It has no web dependencies.
- **`Battleship.Api`** — a thin ASP.NET Core Minimal API that exposes the engine over REST, maps results to HTTP status codes, and serves Swagger docs.
- **`Battleship.Tests`** — xUnit coverage for the engine.
- **`battleship-ui`** — a Vite/React/TypeScript single-page app that talks to the API and renders the boards with SVG ship assets.

---

## Project layout

```
Battleship/
├── Battleship.sln
├── Battleship.Core/            # Game engine (no web dependencies)
│   ├── BoardBits.cs            # UInt128 bit-layout helpers
│   ├── Models/                 # Board, Ship, Game, enums
│   ├── Placement/              # Valid placement-mask generation
│   ├── Validation/             # Player-board validation + constants
│   ├── Engine/                 # MoveProcessor, GameEngine, GameStore
│   ├── AI/                     # Bot placement + targeting
│   └── Serialization/          # Internal state → API board strings
├── Battleship.Api/             # ASP.NET Core Minimal API
│   ├── Program.cs              # DI, CORS, Swagger, endpoints
│   ├── DTOs/                   # Request/response contracts
│   └── Mapping/                # Engine results → HTTP results
├── Battleship.Tests/           # xUnit test suite
├── battleship-ui/              # React + TypeScript client
│   └── src/
│       ├── api/                # Typed API client
│       ├── game/               # Board/ship helpers
│       ├── components/         # Boards, ships, palette
│       └── screens/            # Setup + game screens
└── Docs/                       # Full design specification
```

---

## Game rules

The standard fleet is placed on a 10×10 grid (17 ship squares total):

| Ship        | Length | Placement char |
|-------------|:------:|:--------------:|
| Carrier     |   5    |      `C`       |
| Battleship  |   4    |      `B`       |
| Destroyer   |   3    |      `D`       |
| Submarine   |   3    |      `S`       |
| Patrol Boat |   2    |      `P`       |

- Ships are placed horizontally or vertically, never diagonally, and may not overlap or run off the board.
- Players alternate firing at single cells. A shot is a **miss**, a **hit**, a **sunk** (the shot that completes a ship), or a **win** (the shot that sinks the last enemy ship).
- The first side to sink the opposing fleet wins. The player may optionally let the bot move first.

---

## Board encoding

Boards are exchanged as **100-character strings**, index `0` (top-left) through `99` (bottom-right), row-major. The string is a derived view of the engine's internal bitmaps — never the source of truth.

| Char | Meaning |
|:----:|---------|
| `.`  | Empty / unknown water |
| `C B D S P` | Intact ship segment (own board), or a revealed **sunk** ship on the opponent board |
| `c b d s p` | Damaged (hit) ship segment, own board only |
| `x`  | Hit on an enemy ship that is not yet sunk (opponent board) |
| `o`  | Miss |

Two views are produced per board:

- **Player view** reveals all of your own ship segments plus incoming shots.
- **Opponent view** hides undiscovered cells, shows `x`/`o` for your shots, and only reveals a ship's letters once it is fully sunk.

---

## Getting started

### Prerequisites

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and npm

### 1. Run the API

```bash
dotnet run --project Battleship.Api
```

The API listens on `http://localhost:5181` by default. Swagger UI is available at `http://localhost:5181/swagger`.

### 2. Run the web client

```bash
cd battleship-ui
npm install
npm run dev
```

Vite serves the app at `http://localhost:5173` (the API's CORS policy allows this origin). Open it in a browser, deploy your fleet, and start a game.

> The client's API base URL can be overridden with the `VITE_API_BASE` environment variable; it defaults to `http://localhost:5181`.

---

## API reference

Base URL: `http://localhost:5181`

| Method | Route | Description |
|--------|-------|-------------|
| `GET`  | `/health` | Liveness probe. |
| `GET`  | `/games` | List up to the first 100 games (id + status). |
| `POST` | `/game` | Create a game from a player board. |
| `GET`  | `/game/{gameId}` | Fetch current game state. |
| `POST` | `/game/{gameId}/moves` | Submit a shot; returns updated state (including the bot's reply). |
| `DELETE` | `/game/{gameId}` | Delete a game. |

### Create a game

```http
POST /game
Content-Type: application/json

{
  "playerBoard": "CCCCC.....BBBB..........DDD.......SSS.......PP...... (…100 chars)",
  "botMovesFirst": false
}
```

### Submit a move

```http
POST /game/{gameId}/moves
Content-Type: application/json

{
  "location": 34
}
```

### Game state response

```json
{
  "gameId": "…",
  "status": "in_progress",
  "playerBoard": "…100 chars…",
  "opponentBoard": "…100 chars…",
  "lastPlayerMove": { "location": 34, "result": "hit" },
  "lastBotMove": { "location": 12, "result": "miss" }
}
```

`status` is one of `in_progress`, `player_won`, or `bot_won`. `result` is one of `miss`, `hit`, `sunk`, or `win`.

### Errors

Errors return a JSON body `{ "code": "…", "message": "…" }` with an appropriate status code:

- `400 Bad Request` — invalid board (wrong length, illegal characters, bad fleet, overlapping ships) or out-of-range move.
- `404 Not Found` — unknown game id.
- `409 Conflict` — move on a finished game or a cell already played.

---

## Running the tests

```bash
dotnet test
```

The suite covers bit-layout helpers, placement-mask generation, board validation, move processing, bot placement/targeting, board serialization, and the end-to-end game engine.

---

## Design notes

- **Bitmap board state.** Each board is represented with `UInt128` masks (`BoardBits`), making occupancy, overlap, hit, and sunk checks fast bitwise operations.
- **Precomputed placements.** `PlacementMaskGenerator` enumerates every legal horizontal/vertical position for each ship once at startup; loop bounds guarantee ships never wrap rows or fall off the board. These are reused for both validation and bot placement.
- **Validation lives in the engine.** `PlayerBoardValidator` and `MoveProcessor` own all validation and error semantics; the API layer only maps their results to HTTP. This keeps callers thin and avoids duplicated rules.
- **Deterministic bot.** `BotPlacementService` and `BotTargetingService` accept an injectable `Random`, so tests can seed them for reproducible behavior.
- **Strings are a view, not state.** The 100-character board strings are serialized on demand from the internal bitmaps, with separate player and opponent views.

---

## Specification

The complete design specification lives in [`Docs/`](Docs/). Start with [`Docs/Battleship-Specification.md`](Docs/Battleship-Specification.md), or read the topic-split files (overview, board encoding, API contract, validation rules, internal engine, placement masks, bot AI, UI design, SVG assets, and project structure).
