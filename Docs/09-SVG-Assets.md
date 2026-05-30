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

