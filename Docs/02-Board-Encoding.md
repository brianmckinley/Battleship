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

