import { GameState } from './game-state';
import { GameService } from '../../services/game.service';
export class InProgressState implements GameState {
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