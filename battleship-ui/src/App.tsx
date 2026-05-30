import { useState } from "react";
import { ApiError, createGame, submitMove } from "./api/client";
import type { GameState } from "./api/types";
import { GameScreen } from "./screens/GameScreen";
import { SetupScreen } from "./screens/SetupScreen";

export default function App() {
  const [game, setGame] = useState<GameState | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleStart = async (playerBoard: string, botMovesFirst: boolean) => {
    setBusy(true);
    setError(null);
    try {
      const state = await createGame(playerBoard, botMovesFirst);
      setGame(state);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to create game.");
    } finally {
      setBusy(false);
    }
  };

  const handleFire = async (location: number) => {
    if (!game) return;
    setBusy(true);
    setError(null);
    try {
      const state = await submitMove(game.gameId, location);
      setGame(state);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to submit move.");
    } finally {
      setBusy(false);
    }
  };

  const handleNewGame = () => {
    setGame(null);
    setError(null);
  };

  return (
    <main className="app">
      {game === null ? (
        <SetupScreen onStart={handleStart} busy={busy} error={error} />
      ) : (
        <GameScreen
          game={game}
          busy={busy}
          error={error}
          onFire={handleFire}
          onNewGame={handleNewGame}
        />
      )}
    </main>
  );
}
