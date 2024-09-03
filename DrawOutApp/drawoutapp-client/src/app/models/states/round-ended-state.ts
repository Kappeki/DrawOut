import { GameService } from '../../services/game.service';
import { GameState } from './game-state';

export class RoundEndedState implements GameState {
    constructor(private gameService: GameService) { }

    startRound(): void {
    }

    endRound(): void {
    }

    onTimerTick(): void {
    }

    onGuess(team: 'blue' | 'red', guess: string): void {
    }

}