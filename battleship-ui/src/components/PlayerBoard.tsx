import { DIM, SIZE, parsePlayerBoard } from "../game/board";
import { BoardFrame } from "./BoardFrame";
import { ShipSegment } from "./ShipSegment";

interface PlayerBoardProps {
  board: string;
  lastBotMove: number | null;
}

/** Renders the player's own fleet: ships, damage, and bot misses. */
export function PlayerBoard({ board, lastBotMove }: PlayerBoardProps) {
  const { ships, misses } = parsePlayerBoard(board);
  const missSet = new Set(misses);
  const sunkCellSet = new Set(ships.filter((s) => s.sunk).flatMap((s) => s.cells));

  const cells = Array.from({ length: SIZE }, (_, index) => {
    const isMiss = missSet.has(index);
    const highlight = index === lastBotMove;
    const isSunk = sunkCellSet.has(index);
    return (
      <div
        key={index}
        className={`cell water${isSunk ? " sunk-cell" : ""}${highlight ? " last-move" : ""}`}
      >
        {isMiss && <span className="miss-marker" aria-label="miss" />}
      </div>
    );
  });

  const shipOverlays = ships.flatMap((parsed) =>
    parsed.cells.map((cell, segmentIndex) => (
      <div
        key={`${parsed.ship.type}-${cell}`}
        className={`ship-overlay${parsed.sunk ? " sunk" : ""}`}
        style={{
          gridColumnStart: (cell % DIM) + 1,
          gridRowStart: Math.floor(cell / DIM) + 1,
        }}
      >
        <ShipSegment
          ship={parsed.ship}
          segmentIndex={segmentIndex}
          vertical={parsed.vertical}
          hit={parsed.hits[segmentIndex]}
        />
      </div>
    )),
  );

  return <BoardFrame cells={cells} ships={shipOverlays} />;
}
