import type { ApiErrorResponse, GameState, GameSummary } from "./types";

const API_BASE: string =
  (import.meta.env.VITE_API_BASE as string | undefined) ?? "http://localhost:5181";

export class ApiError extends Error {
  readonly code: string;
  readonly status: number;

  constructor(code: string, message: string, status: number) {
    super(message);
    this.name = "ApiError";
    this.code = code;
    this.status = status;
  }
}

async function parseOrThrow<T>(response: Response): Promise<T> {
  if (response.ok) {
    return (await response.json()) as T;
  }

  let code = "UnknownError";
  let message = `Request failed with status ${response.status}.`;
  try {
    const error = (await response.json()) as ApiErrorResponse;
    code = error.code ?? code;
    message = error.message ?? message;
  } catch {
    // Non-JSON error body; keep defaults.
  }

  throw new ApiError(code, message, response.status);
}

export async function createGame(
  playerBoard: string,
  botMovesFirst: boolean,
): Promise<GameState> {
  const response = await fetch(`${API_BASE}/game`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ playerBoard, botMovesFirst }),
  });
  return parseOrThrow<GameState>(response);
}

export async function submitMove(gameId: string, location: number): Promise<GameState> {
  const response = await fetch(`${API_BASE}/game/${gameId}/moves`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ location }),
  });
  return parseOrThrow<GameState>(response);
}

export async function getGame(gameId: string): Promise<GameState> {
  const response = await fetch(`${API_BASE}/game/${gameId}`);
  return parseOrThrow<GameState>(response);
}

export async function deleteGame(gameId: string): Promise<void> {
  await fetch(`${API_BASE}/game/${gameId}`, { method: "DELETE" });
}

export async function listGames(): Promise<GameSummary[]> {
  const response = await fetch(`${API_BASE}/games`);
  return parseOrThrow<GameSummary[]>(response);
}
