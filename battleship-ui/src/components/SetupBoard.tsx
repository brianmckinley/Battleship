import { SIZE, DIM, type Placements } from "../game/board";
import { SHIPS } from "../game/ships";
import { BoardFrame } from "./BoardFrame";
import { ShipSegment } from "./ShipSegment";

interface SetupBoardProps {
  placements: Placements;
  previewCells: number[] | null;
  previewValid: boolean;
  onCellEnter: (index: number) => void;
  onCellClick: (index: number) => void;
  onDropAt: (index: number) => void;
}

export function SetupBoard({
  placements,
  previewCells,
  previewValid,
  onCellEnter,
  onCellClick,
  onDropAt,
}: SetupBoardProps) {
  const previewSet = new Set(previewCells ?? []);

  const cells = Array.from({ length: SIZE }, (_, index) => {
    let cls = "cell water setup-cell";
    if (previewSet.has(index)) cls += previewValid ? " preview-valid" : " preview-invalid";
    return (
      <div
        key={index}
        className={cls}
        onMouseEnter={() => onCellEnter(index)}
        onClick={() => onCellClick(index)}
        onDragOver={(e) => {
          e.preventDefault();
          onCellEnter(index);
        }}
        onDrop={(e) => {
          e.preventDefault();
          onDropAt(index);
        }}
      />
    );
  });

  const shipOverlays = SHIPS.flatMap((ship) => {
    const placement = placements[ship.type];
    if (!placement) return [];
    return placement.cells.map((cell, segmentIndex) => (
      <div
        key={`${ship.type}-${cell}`}
        className="ship-overlay placed-ship"
        style={{
          gridColumnStart: (cell % DIM) + 1,
          gridRowStart: Math.floor(cell / DIM) + 1,
        }}
        onClick={() => onCellClick(cell)}
        title={`${ship.name} — click to remove`}
      >
        <ShipSegment ship={ship} segmentIndex={segmentIndex} vertical={placement.vertical} hit={false} />
      </div>
    ));
  });

  return <BoardFrame cells={cells} ships={shipOverlays} />;
}
