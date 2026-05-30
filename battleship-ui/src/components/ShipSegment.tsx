import { segmentRole, segmentSvg, type ShipDef } from "../game/ships";

interface ShipSegmentProps {
  ship: ShipDef;
  /** Index of this cell within the ship (0-based, bow first). */
  segmentIndex: number;
  vertical: boolean;
  hit: boolean;
}

/**
 * Renders a single ship segment image (bow / middle / stern), rotated for
 * vertical ships, with an optional damage overlay.
 */
export function ShipSegment({ ship, segmentIndex, vertical, hit }: ShipSegmentProps) {
  const role = segmentRole(segmentIndex, ship.length);
  const src = segmentSvg(ship, role);

  return (
    <div className="ship-segment">
      <img
        className="ship-segment-img"
        src={src}
        alt={`${ship.name} ${role}`}
        style={{ transform: vertical ? "rotate(90deg)" : undefined }}
        draggable={false}
      />
      {hit && <span className="hit-marker" aria-label="hit">✕</span>}
    </div>
  );
}
