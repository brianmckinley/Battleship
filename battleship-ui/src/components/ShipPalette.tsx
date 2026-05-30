import type { Placements } from "../game/board";
import { SHIPS, type ShipType } from "../game/ships";
import { ShipPreview } from "./ShipPreview";

interface ShipPaletteProps {
  placements: Placements;
  selectedType: ShipType | null;
  onSelect: (type: ShipType) => void;
  onDragStart: (type: ShipType) => void;
}

export function ShipPalette({ placements, selectedType, onSelect, onDragStart }: ShipPaletteProps) {
  return (
    <div className="palette">
      <h2>Fleet</h2>
      <ul className="palette-list">
        {SHIPS.map((ship) => {
          const placed = placements[ship.type] !== undefined;
          const selected = selectedType === ship.type;
          return (
            <li
              key={ship.type}
              className={`palette-item${placed ? " placed" : ""}${selected ? " selected" : ""}`}
              draggable={!placed}
              onDragStart={() => onDragStart(ship.type)}
              onClick={() => !placed && onSelect(ship.type)}
            >
              <div className="palette-info">
                <span className="palette-name">{ship.name}</span>
                <span className={`palette-length${placed ? " placed" : ""}`}>
                  {placed ? "Placed" : ship.length}
                </span>
              </div>
              <ShipPreview ship={ship} />
            </li>
          );
        })}
      </ul>
    </div>
  );
}
