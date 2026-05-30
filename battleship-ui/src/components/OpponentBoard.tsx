import { DIM, SIZE, isTargetable, parseOpponentBoard } from "../game/board";
import { BoardFrame } from "./BoardFrame";
import { ShipSegment } from "./ShipSegment";

interface OpponentBoardProps {
  board: string;
  onFire: (index: number) => void;
  disabled: boolean;
  lastPlayerMove: number | null;
}

/** Renders the opponent board the player fires at. */
export function OpponentBoard({ board, onFire, disabled, lastPlayerMove }: OpponentBoardProps) {
  const { sunkShips, hits, misses } = parseOpponentBoard(board);
  const hitSet = new Set(hits);
  const missSet = new Set(misses);
  const sunkCellSet = new Set(sunkShips.flatMap((s) => s.cells));

  const cells = Array.from({ length: SIZE }, (_, index) => {
    const targetable = isTargetable(board, index) && !disabled;
    const isHit = hitSet.has(index);
    const isMiss = missSet.has(index);
    const isSunk = sunkCellSet.has(index);
    const highlight = index === lastPlayerMove;

    return (
      <button
        key={index}
        type="button"
        className={`cell target${isSunk ? " sunk-cell" : ""}${targetable ? " targetable" : ""}${highlight ? " last-move" : ""}`}
        onClick={() => targetable && onFire(index)}
        disabled={!targetable}
        aria-label={`Fire at cell ${index}`}
      >
        {isHit && <span className="hit-marker standalone" aria-label="hit">✕</span>}
        {isMiss && <span className="miss-marker" aria-label="miss" />}
      </button>
    );
  });

  const shipOverlays = sunkShips.flatMap((parsed) =>
    parsed.cells.map((cell, segmentIndex) => (
      <div
        key={`${parsed.ship.type}-${cell}`}
        className="ship-overlay sunk"
        style={{
          gridColumnStart: (cell % DIM) + 1,
          gridRowStart: Math.floor(cell / DIM) + 1,
        }}
      >
        <ShipSegment
          ship={parsed.ship}
          segmentIndex={segmentIndex}
          vertical={parsed.vertical}
          hit
        />
      </div>
    )),
  );

  return <BoardFrame cells={cells} ships={shipOverlays} />;
}
