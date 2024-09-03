import { GameState } from './game-state';
import { GameService } from '../../services/game.service';

export class WaitingForPlayersState implements GameState {
    constructor(private gameService: GameService) { }

    startRound(): void {
    }

    endRound(): void {

    }

    onTimerTick(): void {

    }

    onGuess(team: 'blue' | 'red', guess: string): void {
        console.log('No guessing allowed, waiting for players.');
    }
}


