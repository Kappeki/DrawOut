import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { Game, GameModel, GameRound } from '../models/game';

@Injectable({
  providedIn: 'root'
})
export class GameHubService {
  private readonly API_URL = 'https://localhost:7041';
  public hubConnection: HubConnection = new HubConnectionBuilder()
    .withUrl(`${this.API_URL}/gamehub`, { withCredentials: true })
    .configureLogging(LogLevel.Information)
    .build();

  public mainTimer$ = new BehaviorSubject<number>(0);
  public stealTimer$ = new BehaviorSubject<number>(0);

  public gameModel$ = new BehaviorSubject<GameModel | null>(null);
  public gameRound$ = new BehaviorSubject<GameRound | null>(null);

  public roundWinTeam$ = new BehaviorSubject<string | null>(null);
  public correctWord$ = new BehaviorSubject<string | null>(null);

  public wordSelected$ = new BehaviorSubject<number>(0);

  public enableDrawing$ = new BehaviorSubject<boolean>(false);
  public enableGuessing$ = new BehaviorSubject<boolean>(false);

  public invokeStartNextRound$ = new BehaviorSubject<{ res: boolean, timestamp: number }>({ res: false, timestamp: Date.now() });
  public invokeStartWordSelect$ = new BehaviorSubject<{ res: boolean, timestamp: number }>({ res: false, timestamp: Date.now() });
  public invokeHandleNoWord$ = new BehaviorSubject<{ res: boolean, timestamp: number }>({ res: false, timestamp: Date.now() });
  public invokeStartRound$ = new BehaviorSubject<{ res: boolean, timestamp: number }>({ res: false, timestamp: Date.now() });
  public invokeEndRound$ = new BehaviorSubject<{ res: boolean, timestamp: number }>({ res: false, timestamp: Date.now() });
  public invokeStealPhase$ = new BehaviorSubject<{ res: boolean, timestamp: number }>({ res: false, timestamp: Date.now() });

  public listenPromptWord$ = new BehaviorSubject<{ res: boolean, timestamp: number }>({ res: false, timestamp: Date.now() });
  public listenPainterSelect$ = new BehaviorSubject<{ res: boolean, timestamp: number }>({ res: false, timestamp: Date.now() });


  public messages$ = new BehaviorSubject<any>([]);
  public messages: any[] = [];

  constructor() {
    this.registerListeners();
  }

  public async startConnection() {
    await this.hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch(err => console.error('Error while starting connection: ' + err));
  }
  public async stopConnection() {
    await this.hubConnection
      .stop()
      .then(() => console.log('Connection stopped'))
      .catch(err => console.error('Error while stopping connection: ' + err));
  }
  private registerListeners() {
    this.hubConnection.on('LoadingGame', (gameModel: GameModel, gameRound: GameRound) => {
      console.log(gameModel);
      console.log(gameRound);
      this.gameModel$.next(gameModel);
      this.gameRound$.next(gameRound);
    });
    this.hubConnection.on('GameEnded', (gameModel: GameModel, gameRound: GameRound) => {
      console.log('Game ended.');
      this.gameModel$.next(gameModel);
      this.gameRound$.next(gameRound);
    });
    this.hubConnection.on('ReceiveGameWinner', (team: string) => {
      console.log(`Game won by ${team} team.`);
      this.roundWinTeam$.next(team);
    });
    this.hubConnection.on('RoundStarted', (gameRound: GameRound) => {
      console.log("The round started!!!");
      this.gameRound$.next(gameRound);
    });
    this.hubConnection.on('StealPhaseStarted', (gameRound: GameRound) => {
      console.log('Steal phase started.');
      this.gameRound$.next(gameRound);
    });
    this.hubConnection.on('RoundStandby', (gameRound: GameRound) => {
      console.log('Round standby.');
      this.gameRound$.next(gameRound);
    });
    this.hubConnection.on('RoundEnded', (gameRound: GameRound) => {
      console.log('Round ended.');
      this.gameRound$.next(gameRound);
    });
    this.hubConnection.on('WordSelected', (word: number) => {
      this.wordSelected$.next(word);
    });
    this.hubConnection.on('NoWordSelected', (word: number) => {
      this.wordSelected$.next(word);
    });
    this.hubConnection.on('UpdateMainTimer', (mainTimer: number) => {
      this.mainTimer$.next(mainTimer);
    });
    this.hubConnection.on('UpdateStealTimer', (stealTimer: number) => {
      this.stealTimer$.next(stealTimer);
    });
    this.hubConnection.on('EnableDrawing', (res: boolean) => {
      console.log('Enable drawing: ' + res);
      this.enableDrawing$.next(res);
    });
    this.hubConnection.on('EnableGuessing', (res: boolean) => {
      console.log('Enable guessing: ' + res);
      this.enableGuessing$.next(res);
    });
    this.hubConnection.on('ReceiveCorrectGuess', (msg: string, team: 'blue' | 'red', guess: string) => {
      console.log(`Correct guess by ${team} team.`);
      this.messages$.next([...this.messages, msg]);
      this.roundWinTeam$.next(team);
      this.correctWord$.next(guess);
    });

    this.hubConnection.on('StartingSteal', (res: boolean, timestamp: number) => {
      this.invokeStealPhase$.next({ res, timestamp });
    });

    this.hubConnection.on('StartingRound', (res: boolean, timestamp: number) => {
      this.invokeStartRound$.next({ res, timestamp });
    });

    this.hubConnection.on('HandlingNoWord', (res: boolean, timestamp: number) => {
      this.invokeHandleNoWord$.next({ res, timestamp });
    });

    this.hubConnection.on('StartingNextRound', (res: boolean, timestamp: number) => {
      this.invokeStartNextRound$.next({ res, timestamp });
    });

    this.hubConnection.on('StartingSelect', (res: boolean, timestamp: number) => {
      this.invokeStartWordSelect$.next({ res, timestamp });
    });

    this.hubConnection.on('EndingRound', (res: boolean, timestamp: number) => {
      this.invokeEndRound$.next({ res, timestamp });
    });

    this.hubConnection.on('PromptWordSelection', (res: boolean, timestamp: number) => {
      this.listenPromptWord$.next({ res, timestamp });
    });

    this.hubConnection.on('PainterSelectingWord', (res: boolean, timestamp: number) => {
      this.listenPainterSelect$.next({ res, timestamp });
    });

  }


  public async startNextRound(gameRound: GameRound) {
    return await this.hubConnection.invoke('StartNextRound', gameRound)
      .catch(err => console.error(err));
  }
  public async startRound(gameRound: GameRound) {
    return await this.hubConnection.invoke('StartRound', gameRound)
      .catch(err => console.error(err));
  }
  public async endRound(gameRound: GameRound) {
    return await this.hubConnection.invoke('EndRound', gameRound)
      .catch(err => console.error(err));
  }
  public async handleNoWordSelected() {
    return await this.hubConnection.invoke('HandleNoWordSelected')
      .catch(err => console.error(err));
  }
  public async startStealPhase(gameRound: GameRound) {
    return await this.hubConnection.invoke('StartStealPhase', gameRound)
      .catch(err => console.error(err));
  }


  public async selectWord(gameId: string, selectedWord: string) {
    return await this.hubConnection.invoke('SelectWord', gameId, selectedWord)
      .catch(err => console.error(err));
  }
  public async submitGuess(gameId: string, guess: string) {
    return await this.hubConnection.invoke('SubmitGuess', gameId, guess)
      .catch(err => console.error(err));
  }
  public async startGame(game: Game) {
    console.log('Starting game...');
    console.log(game);

    return await this.hubConnection.invoke('StartGame', game)
      .catch(err => console.error(err));
  }
  public async connectToGame(gameId: string) {
    return await this.hubConnection.invoke('ConnectToGame', gameId)
      .catch(err => console.error(err));
  }

  public async startWordSelection(gameRound: GameRound) {
    return await this.hubConnection.invoke('StartWordSelection', gameRound)
      .catch(err => console.error(err));
  }
}
