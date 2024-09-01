export interface Game {
    blueScore: number;
    redScore: number;
    teamLeaders: { [team: string]: string };
    totalRounds: number;
    painterOrder: string[];
    currentPainter: string;  // The user currently drawing
    selectedWord: string;  // The word being drawn
    currentRound: number;  // The current round number
    mainTimer: number;  // Timer for the drawing phase
    stealTimer: number;  // Timer for the steal phase
  }