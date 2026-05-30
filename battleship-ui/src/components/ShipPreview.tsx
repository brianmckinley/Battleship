import { segmentRole, segmentSvg, type ShipDef } from "../game/ships";

interface ShipPreviewProps {
  ship: ShipDef;
  cellSize?: number;
}

/** A small horizontal rendering of a whole ship, used in the palette. */
export function ShipPreview({ ship, cellSize = 36 }: ShipPreviewProps) {
  return (
    <div className="ship-preview" style={{ width: ship.length * cellSize, height: cellSize }}>
      {Array.from({ length: ship.length }, (_, i) => {
        const role = segmentRole(i, ship.length);
        return (
          <img
            key={i}
            className="ship-preview-segment"
            src={segmentSvg(ship, role)}
            alt={`${ship.name} ${role}`}
            style={{ width: cellSize, height: cellSize }}
            draggable={false}
          />
        );
      })}
    </div>
  );
}
