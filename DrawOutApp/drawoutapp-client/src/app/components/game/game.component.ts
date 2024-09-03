import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, TemplateRef, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { WhiteboardComponent } from '../whiteboard/whiteboard.component';
import { Game, GameModel, GameRound } from '../../models/game';
import { GameService } from '../../services/game.service';
import { User } from '../../models/user';
import { GameHubService } from '../../services/game-hub.service';
import { Observable, Subscription } from 'rxjs';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatOption, MatSelect } from '@angular/material/select';
import { MatButton, MatButtonModule } from '@angular/material/button';
import { SessionService } from '../../services/session.service';
@Component({
  selector: 'app-game',
  standalone: true,
  imports: [CommonModule, FormsModule, WhiteboardComponent, MatDialogModule, MatFormField, MatSelect, MatLabel, MatOption, MatButtonModule, MatButton],
  templateUrl: './game.component.html',
  styleUrl: './game.component.css'
})
export class GameComponent implements OnInit {

  @Input() users: User[] = [];
  @Input() roundTime: number = 0;
  @Input() roomId: string = '';
  @Input() isRoomAdmin: boolean = false;
  @Input() wordPack: string[] = [];
  @Input() chatMessages: any[] = [];
  @Input() latestGuess$!: Observable<string | null>;

  @Output() roundChange = new EventEmitter<{ newRound: number, totalRounds: number }>();
  @Output() guessEnabledChange = new EventEmitter<boolean>();

  @ViewChild('wordSelectionModal', { static: true }) wordSelectionModal!: TemplateRef<any>;

  private subscriptions: Subscription = new Subscription();

  winningTeam: string = '';
  drawEnabled = false;
  guessEnabled = false;
  blankCount = 0;
  game: Game | null = null;
  msgNotificationsUI: any[] = [];
  isPainter: boolean = false;

  isStealState: boolean = false;
  isInProgressState: boolean = false;

  selectedWords: string[] = [];
  hint: string = '';

  private gameModel: GameModel | null = null;
  private gameRound: GameRound | null = null;


  constructor(private gameHubService: GameHubService, public dialog: MatDialog, private sessionService: SessionService) {
  }

  //korisnici ne smeju da joinuju dok je in progress to je poenta
  //chat disable za team na osnvou current painter team


  async ngOnInit(): Promise<void> {

    await this.gameHubService.startConnection().then(() => {
      console.log('GameHub connection started.');
      this.gameHubService.connectToGame(`game:${this.roomId}`);
    });

    if (this.isRoomAdmin) {
      this.game = this.createGameModel();
      console.log(this.wordPack);
      this.gameHubService.startGame(this.game);
    }

    this.subscriptions.add(this.gameHubService.hubConnection.on('UserConnected', (nickname: String) => {
      console.log(`${nickname} connected to the game.`);
    }));

    this.subscriptions.add(this.gameHubService.invokeStealPhase$.subscribe(({ res, timestamp }) => {
      if (res) {
        console.log('Starting steal phase at', timestamp);
        this.isStealState = res;
        this.gameHubService.startStealPhase(this.gameRound!);
      }
    }));

    this.subscriptions.add(this.gameHubService.invokeEndRound$.subscribe(({ res, timestamp }) => {
      if (res) {
        console.log('Ending round at', timestamp);
        this.gameHubService.endRound(this.gameRound!);
      }
    }));

    this.subscriptions.add(this.gameHubService.invokeStartRound$.subscribe(({ res, timestamp }) => {
      if (res) {
        console.log('Starting round at', timestamp);
        this.isInProgressState = res;
        this.gameHubService.startRound(this.gameRound!);
      }
    }));

    this.subscriptions.add(this.gameHubService.invokeHandleNoWord$.subscribe(({ res, timestamp }) => {
      if (res && this.isRoomAdmin) {
        console.log('Handling no word selected at', timestamp);
        this.gameHubService.handleNoWordSelected();
      }
    }));

    this.subscriptions.add(this.gameHubService.invokeStartNextRound$.subscribe(({ res, timestamp }) => {
      if (res && this.isRoomAdmin) {
        console.log('Starting next round at', timestamp);
        this.gameHubService.startNextRound(this.gameRound!);
      }
    }));

    this.subscriptions.add(this.gameHubService.invokeStartWordSelect$.subscribe(({ res, timestamp }) => {
      if (res) {
        console.log('Starting word selection at', timestamp);
        this.gameHubService.startWordSelection(this.gameRound!);
      }
    }));

    this.subscriptions.add(this.gameHubService.gameModel$.subscribe(res => {
      this.gameModel = res!;
      this.consolidateGame(res!, this.gameRound!);
    }));
    this.subscriptions.add(this.gameHubService.gameRound$.subscribe(res => {
      this.gameRound = res;
      this.consolidateGame(this.gameModel!, res!);
    }));
    this.subscriptions.add(this.gameHubService.messages$.subscribe(res => {
      this.msgNotificationsUI = res;
    }));
    this.subscriptions.add(this.gameHubService.wordSelected$.subscribe(res => {
      this.blankCount = res;
      this.hint = "_".repeat(this.blankCount);
    }));
    this.subscriptions.add(this.gameHubService.mainTimer$.subscribe(res => {
      if (this.game) {
        this.game.mainTimer = res;
      } else {
        console.error("Game object is null. Cannot set mainTimer.");
      }
    }));
    this.subscriptions.add(this.gameHubService.stealTimer$.subscribe(res => {
      if (this.game) {
        this.game.stealTimer = res;
      } else {
        console.error("Game object is null. Cannot set stealTimer.");
      }
    }));
    this.subscriptions.add(this.gameHubService.enableDrawing$.subscribe(res => {
      res ? console.log('Drawing enabled') : console.log('Drawing disabled');
      this.drawEnabled = res;
    }));
    this.subscriptions.add(this.gameHubService.enableGuessing$.subscribe(res => {
      res ? console.log('Guessing enabled') : console.log('Guessing disabled');
      this.guessEnabled = res;
      this.guessEnabledChange.emit(res);
    }));
    this.subscriptions.add(this.gameHubService.roundWinTeam$.subscribe(res => {
      this.winningTeam = res!;
    }));
    this.subscriptions.add(this.gameHubService.correctWord$.subscribe(res => {
      this.hint = res!;
    }));


    this.subscriptions.add(this.gameHubService.listenPromptWord$.subscribe(({ res, timestamp }) => {
      console.log(this.game?.currentPainter);
      console.log(this.sessionService.getSessionId());
      if (this.game?.currentPainter === this.sessionService.getSessionId()) {
        console.log('Im choosing a word at', timestamp);
        this.prepareWordSelection();
        this.showWordSelectModal();
        this.isPainter = res;
      }
    }));

    this.subscriptions.add(this.gameHubService.listenPainterSelect$.subscribe(({ res, timestamp }) => {
      if (this.game?.currentPainter !== this.sessionService.getSessionId()) {
        console.log('Painter is choosing a word at', timestamp);
        this.showPainterSelectOverlay();
        this.isPainter = !res;
      }
    }));

    this.subscriptions.add(
      this.latestGuess$.subscribe(guess => {
        if (guess) {
          this.submitGuess(guess);
        }
      }));
  }

  ngOnDestroy(): void {
    this.game = null;
    this.subscriptions.unsubscribe();
    this.gameHubService.stopConnection().then(() => {
      console.log('GameHub connection stopped.');
    });
  }

  // ngAfterViewInit() {
  //   if (this.isPainter) {
  //     this.showWordSelectModal();
  //   }
  // }

  private prepareWordSelection(): void {
    if (this.wordPack && this.wordPack.length >= 4) {
      this.selectedWords = [];
      const shuffled = [...this.wordPack].sort(() => 0.5 - Math.random());
      this.selectedWords = shuffled.slice(0, 4);
    }
  }

  selectWord(word: string) {
    this.gameHubService.selectWord(`game:${this.roomId}`, word);
    this.dialog.closeAll();
  }

  submitGuess(guess: string) {
    this.gameHubService.submitGuess(`game:${this.roomId}`, guess);
  }

  private showWordSelectModal(): void {
    if (this.selectedWords.length > 0) {
      this.dialog.open(this.wordSelectionModal, {
        data: { words: this.selectedWords },
        disableClose: true
      });
    }
  }
  private showPainterSelectOverlay(): void {
    const overlay = document.createElement('div');
    overlay.className = 'painter-selecting-overlay';
    overlay.innerText = 'Painter is selecting a word...';
    document.querySelector('.game-container')!.appendChild(overlay);
  }
  private consolidateGame(gameModel: GameModel, gameRound: GameRound) {
    if (gameModel && gameRound) {
      const game: Game = {
        _id: gameModel._id,
        roomId: gameModel.roomId,
        teamLeaders: gameModel.teamLeaders,
        painterOrder: gameModel.painterOrder,
        totalRounds: gameModel.totalRounds,
        gameState: gameRound.gameState,
        blueScore: gameRound.blueScore,
        redScore: gameRound.redScore,
        currentRound: gameRound.currentRound,
        currentPainter: gameRound.currentPainter,
        selectedWord: gameRound.selectedWord,
        mainTimer: gameRound.mainTimer,
        stealTimer: gameRound.stealTimer,
      }
      this.game = game;
    }
  }
  private createGameModel(): Game {
    const painterOrder = this.determinePainterOrder(this.users);
    const teamLeaders = this.selectTeamLeaders(this.users);
    const totalRounds = painterOrder.length;

    return {
      _id: `game:${this.roomId}`,
      roomId: this.roomId,
      teamLeaders: teamLeaders,
      painterOrder: painterOrder,
      totalRounds: totalRounds,
      blueScore: 0,
      redScore: 0,
      currentRound: 0,
      currentPainter: painterOrder[0],
      selectedWord: '',
      mainTimer: this.roundTime,
      stealTimer: this.roundTime / 2
    };
  }
  private determinePainterOrder(users: User[]): string[] {
    const redTeam = users.filter(user => user.roles!.includes('Red'));
    const blueTeam = users.filter(user => user.roles!.includes('Blue'));

    const combinedOrder: string[] = [];
    const firstTeam = Math.random() < 0.5 ? redTeam : blueTeam;
    const secondTeam = firstTeam === redTeam ? blueTeam : redTeam;

    let i = 0;
    let j = 0;

    while (i < firstTeam.length || j < secondTeam.length) {
      if (i < firstTeam.length) {
        combinedOrder.push(firstTeam[i]._sessionKey);
        i++;
      }
      if (j < secondTeam.length) {
        combinedOrder.push(secondTeam[j]._sessionKey);
        j++;
      }
    }

    return combinedOrder;
  }
  private selectTeamLeaders(users: User[]): { [team: string]: string } {
    const redTeam = users.filter(user => user.roles!.includes('Red'));
    const blueTeam = users.filter(user => user.roles!.includes('Blue'));

    const redLeader = redTeam[Math.floor(Math.random() * redTeam.length)];
    const blueLeader = blueTeam[Math.floor(Math.random() * blueTeam.length)];

    return {
      Red: redLeader._sessionKey,
      Blue: blueLeader._sessionKey,
    };
  }
}
