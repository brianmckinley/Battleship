import type { ReactNode } from "react";

interface BoardFrameProps {
  /** The 10×10 grid of cells (auto-placed). */
  cells: ReactNode;
  /** Explicitly-positioned ship overlays, rendered on a separate layer. */
  ships?: ReactNode;
}

/**
 * Stacks the cell grid and the ship overlay grid in separate layers so that
 * explicitly-positioned ships never displace auto-placed cells.
 */
export function BoardFrame({ cells, ships }: BoardFrameProps) {
  return (
    <div className="board-frame">
      <div className="board cells-grid">{cells}</div>
      {ships && <div className="board ship-grid">{ships}</div>}
    </div>
  );
}
