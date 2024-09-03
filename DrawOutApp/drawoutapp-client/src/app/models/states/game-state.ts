export interface GameState {
    startRound(): void;
    endRound(): void;
    onTimerTick(): void;
    onGuess(team: 'blue' | 'red', guess: string): void;
}