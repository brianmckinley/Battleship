export type MoveResultValue = "miss" | "hit" | "sunk" | "win";

export type GameStatus = "in_progress" | "player_won" | "bot_won";

export interface MoveResult {
  location: number;
  result: MoveResultValue;
}

export interface GameState {
  gameId: string;
  status: GameStatus;
  playerBoard: string;
  opponentBoard: string;
  lastPlayerMove: MoveResult | null;
  lastBotMove: MoveResult | null;
}

export interface GameSummary {
  gameId: string;
  status: string;
}

export interface ApiErrorResponse {
  message: string;
  code: string;
}
