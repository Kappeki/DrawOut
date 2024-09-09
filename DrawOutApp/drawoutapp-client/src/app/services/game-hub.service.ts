import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { GameModelView, GameRoundView } from '../models/game';

@Injectable({
  providedIn: 'root'
})
export class GameHubService {
  private readonly API_URL = 'https://localhost:7041';
  public hubConnection: HubConnection = new HubConnectionBuilder()
    .withUrl(`${this.API_URL}/gamehub`, { withCredentials: true })
    .configureLogging(LogLevel.Information)
    .build();

  public gameModel$ = new BehaviorSubject<GameModelView | null>(null);
  public gameRound$ = new BehaviorSubject<GameRoundView | null>(null);

  public wordSelected$ = new BehaviorSubject<number>(0);
  public roundWinTeam$ = new BehaviorSubject<string>('');

  public timer$ = new BehaviorSubject<number>(0);
  public currentTimer$ = new BehaviorSubject<string>('');
  public previousTimer$ = new BehaviorSubject<string>('');

  constructor() {
    //EnableGuessing, EnableDrawing, PromptWordSelect fali, mozda u game comp direktno


  }

  public async startConnection() {
    await this.hubConnection
      .start()
      .then(() => {
        console.log('Connection started')
        this.setupListeners();
      })
      .catch(err => console.error('Error while starting connection: ' + err));
  }
  public async stopConnection() {
    await this.hubConnection
      .stop()
      .then(() => console.log('Connection stopped'))
      .catch(err => console.error('Error while stopping connection: ' + err));
  }

  public async startGame() {
    await this.hubConnection.invoke('StartGame')
      .catch(err => console.error(err));
  }

  public async connectToGame(gameId: string) {
    await this.hubConnection.invoke('ConnectToGame', gameId)
      .catch(err => console.error(err));
  }

  public async selectWord(word: string) {
    await this.hubConnection.invoke('SelectWord', word)
      .catch(err => console.error(err));
  }

  public async submitGuess(guess: string) {
    await this.hubConnection.invoke('SubmitGuess', guess)
      .catch(err => console.error(err));
  }

  private setupListeners(): void {
    this.hubConnection.on('LoadGame', (gameModel: GameModelView, gameRound: GameRoundView) => {
      this.gameModel$.next(gameModel);
      this.gameRound$.next(gameRound);
    });
    this.hubConnection.on('UpdateGame', (gameRound: GameRoundView) => {
      this.gameRound$.next(gameRound);
    });
    this.hubConnection.on('WordSelected', (wordLength: number) => {
      this.wordSelected$.next(wordLength);
    });
    this.hubConnection.on('CorrectGuess', (teamName: string) => {
      this.roundWinTeam$.next(teamName);
    });

    //u game component check za koji timer je aktuelan
    this.hubConnection.on('TimerStarted', (timerName: string, timerTime: number) => {
      this.currentTimer$.next(timerName);
      this.timer$.next(timerTime);
    });
    this.hubConnection.on('TimerUpdate', (timerTime: number) => {
      this.timer$.next(timerTime);
    });
    this.hubConnection.on('TimerStopped', (timerName: string) => {
      this.currentTimer$.next('');
      this.previousTimer$.next(timerName);
    });
  }

}
