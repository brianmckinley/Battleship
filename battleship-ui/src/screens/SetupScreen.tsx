import { useEffect, useMemo, useState } from "react";
import { SetupBoard } from "../components/SetupBoard";
import { ShipPalette } from "../components/ShipPalette";
import {
  allShipsPlaced,
  buildBoardString,
  occupiedCells,
  placementCells,
  randomPlacements,
  type Placements,
} from "../game/board";
import { SHIPS, SHIP_BY_TYPE, type ShipType } from "../game/ships";

interface SetupScreenProps {
  onStart: (playerBoard: string, botMovesFirst: boolean) => void;
  busy: boolean;
  error: string | null;
}

function firstUnplaced(placements: Placements): ShipType | null {
  return SHIPS.find((ship) => placements[ship.type] === undefined)?.type ?? null;
}

export function SetupScreen({ onStart, busy, error }: SetupScreenProps) {
  const [placements, setPlacements] = useState<Placements>({});
  const [vertical, setVertical] = useState(false);
  const [selectedType, setSelectedType] = useState<ShipType | null>("carrier");
  const [hoverStart, setHoverStart] = useState<number | null>(null);
  const [botMovesFirst, setBotMovesFirst] = useState(false);

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "r" || e.key === "R") setVertical((v) => !v);
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, []);

  const preview = useMemo(() => {
    if (selectedType === null || hoverStart === null) return null;
    const ship = SHIP_BY_TYPE[selectedType];
    const cells = placementCells(hoverStart, ship.length, vertical);
    if (!cells) return { cells: [], valid: false };

    const occupied = occupiedCells(placements);
    const valid = cells.every((cell) => !occupied.has(cell));
    return { cells, valid };
  }, [selectedType, hoverStart, vertical, placements]);

  const placeShip = (type: ShipType, start: number) => {
    const ship = SHIP_BY_TYPE[type];
    const cells = placementCells(start, ship.length, vertical);
    if (!cells) return;

    const occupied = occupiedCells(placements);
    if (cells.some((cell) => occupied.has(cell))) return;

    const next: Placements = { ...placements, [type]: { cells, vertical } };
    setPlacements(next);
    setSelectedType(firstUnplaced(next));
    setHoverStart(null);
  };

  const removeShipAt = (index: number) => {
    const entry = SHIPS.find((ship) => placements[ship.type]?.cells.includes(index));
    if (!entry) return;
    const next = { ...placements };
    delete next[entry.type];
    setPlacements(next);
    setSelectedType(entry.type);
  };

  const handleCellClick = (index: number) => {
    const occupied = occupiedCells(placements);
    if (occupied.has(index)) {
      removeShipAt(index);
      return;
    }
    if (selectedType !== null) {
      placeShip(selectedType, index);
    }
  };

  const handleDropAt = (index: number) => {
    if (selectedType !== null) placeShip(selectedType, index);
  };

  const ready = allShipsPlaced(placements);

  return (
    <div className="setup-screen">
      <header className="screen-header">
        <h1>Deploy Your Fleet</h1>
        <p className="subtitle">
          Drag a ship onto the grid or select it and click a cell. Press <kbd>R</kbd> or use Rotate
          to change orientation. Click a placed ship to remove it.
        </p>
      </header>

      <div className="setup-layout">
        <ShipPalette
          placements={placements}
          selectedType={selectedType}
          onSelect={setSelectedType}
          onDragStart={setSelectedType}
        />

        <div className="setup-board-area">
          <SetupBoard
            placements={placements}
            previewCells={preview?.cells ?? null}
            previewValid={preview?.valid ?? false}
            onCellEnter={setHoverStart}
            onCellClick={handleCellClick}
            onDropAt={handleDropAt}
          />

          <div className="setup-controls">
            <button type="button" onClick={() => setVertical((v) => !v)}>
              Rotate: {vertical ? "Vertical" : "Horizontal"}
            </button>
            <button type="button" onClick={() => setPlacements(randomPlacements())}>
              Auto-place
            </button>
            <button
              type="button"
              onClick={() => {
                setPlacements({});
                setSelectedType("carrier");
              }}
            >
              Clear
            </button>
            <label className="checkbox">
              <input
                type="checkbox"
                checked={botMovesFirst}
                onChange={(e) => setBotMovesFirst(e.target.checked)}
              />
              Bot moves first
            </label>
          </div>

          <button
            type="button"
            className="primary start-button"
            disabled={!ready || busy}
            onClick={() => onStart(buildBoardString(placements), botMovesFirst)}
          >
            {busy ? "Starting…" : "Start Game"}
          </button>

          {error && <p className="error-text">{error}</p>}
        </div>
      </div>
    </div>
  );
}
