import { GameState } from './game-state';
import { GameService } from '../../services/game.service';

export class StealState implements GameState {
    constructor(private gameService: GameService) { }

    startRound(): void {
        console.log('Cannot start round, in steal phase.');
    }

    endRound(): void {

    }

    onTimerTick(): void {

    }

    onGuess(team: 'blue' | 'red', guess: string): void {

    }
}