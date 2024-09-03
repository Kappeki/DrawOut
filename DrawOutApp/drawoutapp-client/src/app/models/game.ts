export interface Game {
  _id?: string;
  roomId?: string;
  painterOrder?: string[];
  teamLeaders?: { [team: string]: string };
  totalRounds: number;
  blueScore: number;
  redScore: number;
  gameState?: string;
  currentPainter?: string;  // The user currently drawing
  selectedWord?: string;  // The word being drawn
  currentRound: number;  // The current round number
  mainTimer: number;  // Timer for the drawing phase
  stealTimer: number;  // Timer for the steal phase
}

export interface GameModel {
  _id: string;
  roomId: string;
  teamLeaders: { [team: string]: string };
  painterOrder: string[];
  totalRounds: number;
}

export interface GameRound {
  _id: string;
  roomId: string;
  blueScore: number;
  redScore: number;
  gameState: string;
  currentRound: number;
  currentPainter: string;
  selectedWord: string;
  mainTimer: number;
  stealTimer: number;
}