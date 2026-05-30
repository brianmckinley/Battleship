import { useEffect, useState } from "react";
import { OpponentBoard } from "../components/OpponentBoard";
import { PlayerBoard } from "../components/PlayerBoard";
import type { GameState, MoveResult } from "../api/types";
import { parseOpponentBoard, parsePlayerBoard } from "../game/board";

interface GameScreenProps {
  game: GameState;
  busy: boolean;
  error: string | null;
  onFire: (index: number) => void;
  onNewGame: () => void;
}

function describeMove(label: string, move: MoveResult | null): string {
  if (!move) return `${label}: —`;
  return `${label}: cell ${move.location} → ${move.result.toUpperCase()}`;
}

/** Finds the name of the ship occupying a location among the given parsed ships. */
function shipNameAt(
  ships: { ship: { name: string }; cells: number[] }[],
  location: number,
): string {
  return ships.find((s) => s.cells.includes(location))?.ship.name ?? "ship";
}

/** Derives a sink/win announcement from the latest moves, if any. */
function announcementFor(game: GameState): string | null {
  const player = game.lastPlayerMove;
  if (player && (player.result === "sunk" || player.result === "win")) {
    const name = shipNameAt(parseOpponentBoard(game.opponentBoard).sunkShips, player.location);
    return `You sank the enemy ${name}!`;
  }

  const bot = game.lastBotMove;
  if (bot && (bot.result === "sunk" || bot.result === "win")) {
    const sunk = parsePlayerBoard(game.playerBoard).ships.filter((s) => s.sunk);
    const name = shipNameAt(sunk, bot.location);
    return `The enemy sank your ${name}!`;
  }

  return null;
}

export function GameScreen({ game, busy, error, onFire, onNewGame }: GameScreenProps) {
  const gameOver = game.status !== "in_progress";
  const playerWon = game.status === "player_won";

  const [toast, setToast] = useState<string | null>(null);

  useEffect(() => {
    const message = announcementFor(game);
    if (!message) return;

    setToast(message);
    const timer = window.setTimeout(() => setToast(null), 2600);
    return () => window.clearTimeout(timer);
  }, [game]);

  return (
    <div className="game-screen">
      {toast && <div className="toast" role="status">{toast}</div>}

      <header className="screen-header">
        <h1>Battle Stations</h1>
        <div className="move-log">
          <span className={`move-chip player ${game.lastPlayerMove?.result ?? ""}`}>
            {describeMove("Your shot", game.lastPlayerMove)}
          </span>
          <span className={`move-chip bot ${game.lastBotMove?.result ?? ""}`}>
            {describeMove("Bot shot", game.lastBotMove)}
          </span>
        </div>
      </header>

      <div className="boards">
        <section className="board-panel">
          <h2>Opponent Waters</h2>
          <OpponentBoard
            board={game.opponentBoard}
            onFire={onFire}
            disabled={gameOver || busy}
            lastPlayerMove={game.lastPlayerMove?.location ?? null}
          />
        </section>

        <section className="board-panel">
          <h2>Your Fleet</h2>
          <PlayerBoard
            board={game.playerBoard}
            lastBotMove={game.lastBotMove?.location ?? null}
          />
        </section>
      </div>

      {error && <p className="error-text">{error}</p>}

      {gameOver && (
        <div className="game-over-overlay">
          <div className="game-over-card">
            <h2 className={playerWon ? "win" : "lose"}>{playerWon ? "You Win" : "You Lose"}</h2>
            <button type="button" className="primary" onClick={onNewGame}>
              New Game
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
