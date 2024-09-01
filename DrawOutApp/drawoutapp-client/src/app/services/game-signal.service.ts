import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Game } from '../models/game';

@Injectable({
  providedIn: 'root'
})
export class GameSignalService {
  private gameSubject = new BehaviorSubject<Game>({
    blueScore: 0,
    redScore: 1,
    teamLeaders: {},
    totalRounds: 8,  // Default total rounds
    painterOrder: [],
    currentPainter: '',
    selectedWord: 'Proba',
    currentRound: 1,
    mainTimer: 60,  // Default timer in seconds
    stealTimer: 15,  // Default steal timer in seconds
  });

  game$ = this.gameSubject.asObservable();

  constructor() {}

  updateGame(game: Game) {
    this.gameSubject.next(game);
  }

  startNewRound() {
    const currentGame = this.gameSubject.value;
    currentGame.currentRound += 1;
    currentGame.currentPainter = this.getNextPainter();
    currentGame.selectedWord = this.getNewWord();
    currentGame.mainTimer = 60;  // Reset the main timer
    currentGame.stealTimer = 15;  // Reset the steal timer
    this.updateGame(currentGame);
  }

  private getNextPainter(): string {
    // Logic to get the next painter from the painterOrder
    const currentPainterIndex = this.gameSubject.value.painterOrder.indexOf(this.gameSubject.value.currentPainter);
    const nextIndex = (currentPainterIndex + 1) % this.gameSubject.value.painterOrder.length;
    return this.gameSubject.value.painterOrder[nextIndex];
  }

  private getNewWord(): string {
    // Logic to randomly select a new word from a list of words
    const words = ['apple', 'banana', 'car', 'dog'];  // Example word list
    return words[Math.floor(Math.random() * words.length)];
  }

  handleCorrectGuess(team: 'blue' | 'red') {
    // this.updateScore(team, 1);
    this.startNewRound();
  }

  handleTimerExpiration() {
    // Logic to start the steal timer and handle the steal phase
    const currentGame = this.gameSubject.value;
    currentGame.stealTimer = 15;  // Start the steal timer
    this.updateGame(currentGame);
    // You may need to implement more logic for the steal phase here
  }
}
