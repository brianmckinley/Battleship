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

