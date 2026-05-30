import { SHIP_BY_CHAR, SHIPS, type ShipDef, type ShipType } from "./ships";

export const DIM = 10;
export const SIZE = 100;

export const rowOf = (index: number): number => Math.floor(index / DIM);
export const colOf = (index: number): number => index % DIM;
export const indexOf = (row: number, col: number): number => row * DIM + col;

/**
 * Returns the board indices a ship would occupy starting at `start`, or null
 * if it would run off the board edge (rows never wrap).
 */
export function placementCells(
  start: number,
  length: number,
  vertical: boolean,
): number[] | null {
  const startRow = rowOf(start);
  const startCol = colOf(start);
  const cells: number[] = [];

  for (let k = 0; k < length; k++) {
    if (vertical) {
      const row = startRow + k;
      if (row >= DIM) return null;
      cells.push(indexOf(row, startCol));
    } else {
      const col = startCol + k;
      if (col >= DIM) return null;
      cells.push(indexOf(startRow, col));
    }
  }

  return cells;
}

export interface Placement {
  cells: number[];
  vertical: boolean;
}

export type Placements = Partial<Record<ShipType, Placement>>;

/** Builds the 100-character player board string from current placements. */
export function buildBoardString(placements: Placements): string {
  const board = new Array<string>(SIZE).fill(".");

  for (const ship of SHIPS) {
    const placement = placements[ship.type];
    if (!placement) continue;
    for (const cell of placement.cells) {
      board[cell] = ship.char;
    }
  }

  return board.join("");
}

export function occupiedCells(placements: Placements): Set<number> {
  const occupied = new Set<number>();
  for (const ship of SHIPS) {
    const placement = placements[ship.type];
    if (!placement) continue;
    for (const cell of placement.cells) {
      occupied.add(cell);
    }
  }
  return occupied;
}

export function allShipsPlaced(placements: Placements): boolean {
  return SHIPS.every((ship) => placements[ship.type] !== undefined);
}

export interface ParsedShip {
  ship: ShipDef;
  cells: number[];
  vertical: boolean;
  /** Per-cell hit flag, aligned with `cells`. */
  hits: boolean[];
  /** True when every segment has been hit. */
  sunk: boolean;
}

export interface ParsedPlayerBoard {
  ships: ParsedShip[];
  misses: number[];
}

export interface ParsedOpponentBoard {
  /** Fully sunk (revealed) ships. */
  sunkShips: ParsedShip[];
  /** Hits on still-unsunk ships. */
  hits: number[];
  misses: number[];
}

function collectShipCells(board: string): Map<string, number[]> {
  const byChar = new Map<string, number[]>();
  for (let i = 0; i < board.length; i++) {
    const upper = board[i].toUpperCase();
    if (SHIP_BY_CHAR[upper]) {
      const list = byChar.get(upper) ?? [];
      list.push(i);
      byChar.set(upper, list);
    }
  }
  return byChar;
}

function isVertical(cells: number[]): boolean {
  return cells.length > 1 && cells[1] - cells[0] === DIM;
}

export function parsePlayerBoard(board: string): ParsedPlayerBoard {
  const ships: ParsedShip[] = [];
  const byChar = collectShipCells(board);

  for (const [char, cells] of byChar) {
    cells.sort((a, b) => a - b);
    const hits = cells.map((cell) => board[cell] === board[cell].toLowerCase());
    ships.push({
      ship: SHIP_BY_CHAR[char],
      cells,
      vertical: isVertical(cells),
      hits,
      sunk: hits.every(Boolean),
    });
  }

  const misses: number[] = [];
  for (let i = 0; i < board.length; i++) {
    if (board[i] === "o") misses.push(i);
  }

  return { ships, misses };
}

export function parseOpponentBoard(board: string): ParsedOpponentBoard {
  const sunkShips: ParsedShip[] = [];
  const byChar = collectShipCells(board);

  for (const [char, cells] of byChar) {
    cells.sort((a, b) => a - b);
    sunkShips.push({
      ship: SHIP_BY_CHAR[char],
      cells,
      vertical: isVertical(cells),
      hits: cells.map(() => true),
      sunk: true,
    });
  }

  const hits: number[] = [];
  const misses: number[] = [];
  for (let i = 0; i < board.length; i++) {
    if (board[i] === "x") hits.push(i);
    else if (board[i] === "o") misses.push(i);
  }

  return { sunkShips, hits, misses };
}

/** True when an opponent cell can still be fired at. */
export function isTargetable(board: string, index: number): boolean {
  return board[index] === ".";
}

/** Randomly places the full fleet without overlaps (for the Auto-place button). */
export function randomPlacements(): Placements {
  const placements: Placements = {};
  const occupied = new Set<number>();

  for (const ship of SHIPS) {
    let placed = false;
    while (!placed) {
      const vertical = Math.random() < 0.5;
      const start = Math.floor(Math.random() * SIZE);
      const cells = placementCells(start, ship.length, vertical);
      if (!cells) continue;
      if (cells.some((cell) => occupied.has(cell))) continue;

      placements[ship.type] = { cells, vertical };
      cells.forEach((cell) => occupied.add(cell));
      placed = true;
    }
  }

  return placements;
}
