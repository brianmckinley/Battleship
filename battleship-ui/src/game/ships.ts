export type ShipType =
  | "carrier"
  | "battleship"
  | "destroyer"
  | "submarine"
  | "patrolboat";

export interface ShipDef {
  type: ShipType;
  name: string;
  length: number;
  /** Uppercase placement character used in the API board string. */
  char: string;
  middleSvg: string;
}

export const BOW_SVG = "/svg/bow.svg";
export const STERN_SVG = "/svg/stern.svg";

export const SHIPS: ShipDef[] = [
  { type: "carrier", name: "Carrier", length: 5, char: "C", middleSvg: "/svg/carrier-middle.svg" },
  { type: "battleship", name: "Battleship", length: 4, char: "B", middleSvg: "/svg/battleship-middle.svg" },
  { type: "destroyer", name: "Destroyer", length: 3, char: "D", middleSvg: "/svg/destroyer-middle.svg" },
  { type: "submarine", name: "Submarine", length: 3, char: "S", middleSvg: "/svg/submarine-middle.svg" },
  { type: "patrolboat", name: "Patrol Boat", length: 2, char: "P", middleSvg: "/svg/patrolboat-middle.svg" },
];

export const SHIP_BY_TYPE: Record<ShipType, ShipDef> = Object.fromEntries(
  SHIPS.map((ship) => [ship.type, ship]),
) as Record<ShipType, ShipDef>;

export const SHIP_BY_CHAR: Record<string, ShipDef> = Object.fromEntries(
  SHIPS.map((ship) => [ship.char, ship]),
);

export type SegmentRole = "bow" | "middle" | "stern";

/** Segment role for the i-th cell of a ship of the given length. */
export function segmentRole(index: number, length: number): SegmentRole {
  if (index === 0) return "bow";
  if (index === length - 1) return "stern";
  return "middle";
}

export function segmentSvg(ship: ShipDef, role: SegmentRole): string {
  if (role === "bow") return BOW_SVG;
  if (role === "stern") return STERN_SVG;
  return ship.middleSvg;
}
