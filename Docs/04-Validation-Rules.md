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

