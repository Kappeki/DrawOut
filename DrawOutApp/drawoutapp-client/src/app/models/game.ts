export interface GameModelView {
  _id: string;
  roomId: string;
  teamLeaders: { [team: string]: string };
  painterOrder: string[];
  totalRounds: number;
  mainTimer: number;
  stealTimer: number;
}

export interface GameRoundView {
  _id: string;
  roomId: string;
  blueScore: number;
  redScore: number;
  gameState: string;
  currentRound: number;
  currentPainter: string;
  selectedWord: string;
}