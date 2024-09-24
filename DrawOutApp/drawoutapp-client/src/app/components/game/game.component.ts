import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, TemplateRef, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { WhiteboardComponent } from '../whiteboard/whiteboard.component';
import { GameModelView, GameRoundView } from '../../models/game';
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
  @Input() roomId: string = '';
  @Input() isRoomAdmin: boolean = false;
  @Input() wordPack: string[] = [];
  @Input() latestGuess$!: Observable<string | null>;

  @Output() roundChange = new EventEmitter<{ newRound: number, totalRounds: number }>();
  @Output() guessEnabledChange = new EventEmitter<boolean>();
  @Output() chatClear = new EventEmitter<string[]>();
  @Output() roomStateChange = new EventEmitter<string>();

  @ViewChild('wordSelectionModal', { static: true }) wordSelectionModal!: TemplateRef<any>;
  @ViewChild(WhiteboardComponent) whiteboard!: WhiteboardComponent;


  private subscriptions: Subscription = new Subscription();

  private autoCloseTimeout: any;

  currentTimer = 0;
  currentTimerName = '';
  previousTimerName = '';
  winningTeam: string = '';
  drawEnabled = true;
  guessEnabled = false;
  blankCount = 0;
  msgNotificationsUI: any[] = [];
  notificationText: string = '';

  isPainter: boolean = false;
  gameEnded: boolean = false;

  selectables: string[] = [];
  hint: string = '';

  gameModelView: GameModelView | null = null;
  gameRoundView: GameRoundView | null = null;

  constructor(private gameHubService: GameHubService, public dialog: MatDialog, private sessionService: SessionService) {
  }

  async ngOnInit(): Promise<void> {
    this.gameEnded = false;

    await this.gameHubService.startConnection().then(() => {
      console.log('GameHub connection started.');

      this.subscriptions.add(this.gameHubService.gameModel$.subscribe(res => {
        console.log(res);
        this.gameModelView = res!;
      }));

      this.subscriptions.add(this.gameHubService.gameRound$.subscribe(res => {
        console.log(res);
        this.gameRoundView = res!;
        if (this.gameRoundView.gameState === 'Standby') {
          this.chatClear.emit([]);
          this.displayNotification('Get ready for the next round!', 3000);
        }
        else if (this.gameRoundView.gameState === 'InProgress') {
          this.chatClear.emit([]);
          this.displayNotification('Round started!');
        }
        else if (this.gameRoundView.gameState === 'Steal') {
          this.chatClear.emit([]);
          this.displayNotification('Steal time!');
        }
        else if (this.gameRoundView.gameState === 'RoundEnded') {
          if(this.winningTeam === '') {
            this.displayNotification('Round ended!');
          }
          else {
            this.displayNotification(`${this.winningTeam} won the round!`);
          }
          this.whiteboard.clearCanvas();
        }
        else if (this.gameRoundView.gameState === 'WaitingForPlayers') {
          this.whiteboard.clearCanvas();
        }
      }));

      this.subscriptions.add(this.gameHubService.wordSelected$.subscribe((hint: string) => {
        console.log('Word selected with length: ' + hint.length);
        this.hint = hint;
      }));
      this.subscriptions.add(this.gameHubService.roundWinTeam$.subscribe((teamName: string) => {
        this.winningTeam = teamName;
      }));
      this.subscriptions.add(this.gameHubService.timer$.subscribe((timerTime: number) => {
        this.currentTimer = timerTime;
      }));
      this.subscriptions.add(this.gameHubService.currentTimer$.subscribe((timerName: string) => {
        this.currentTimerName = timerName;
      }));
      this.subscriptions.add(this.gameHubService.previousTimer$.subscribe((timerName: string) => {
        this.previousTimerName = timerName;
      }));

      this.subscriptions.add(
        this.gameHubService.hubConnection.on('EnableGuessing', (res: boolean, timestamp: number) => {
          this.guessEnabled = res;
          this.guessEnabledChange.emit(this.guessEnabled);
        }));
      this.subscriptions.add(
        this.gameHubService.hubConnection.on('EnableDrawing', (res: boolean, timestamp: number) => {
          this.drawEnabled = res;
        }));
      this.subscriptions.add(
        this.gameHubService.hubConnection.on('GameEnded', (roomState: string, team: string) => {
          if (roomState) {
            this.gameEnded = true;
            if(team === 'Tie') {
              this.displayNotification('It\'s a tie!', 5000, roomState);
            } else {
              this.displayNotification(`${team} won the game!`, 5000, roomState);
            }
            this.guessEnabledChange.emit(true);
          }
        }));

      let lastPromptTime = 0;
      this.subscriptions.add(
        this.gameHubService.hubConnection.on('PromptWordSelect', (res: boolean, timestamp: number) => {
          const now = Date.now();
          if (now - lastPromptTime < 1000) {
            return;
          }
          lastPromptTime = now;
          if (res && this.gameRoundView?.currentPainter === this.sessionService.getSessionId()) {
            this.prepareWordSelection();
            setTimeout(() => {
              this.showWordSelectModal();
            }, 3000);
            this.isPainter = true;
          }
          else {
            const painterNickname = this.users.find(u => u._sessionKey === this.gameRoundView?.currentPainter)?.nickname!;
            this.displayNotification(`${painterNickname} is selecting a word...`);
            this.isPainter = false;
          }
        }));

      this.subscriptions.add(
        this.latestGuess$.subscribe(res => {
          if (res) {
            this.gameHubService.submitGuess(res);
          }
        }));

      this.subscriptions.add(
        this.gameHubService.hubConnection.on('UserLeft', (timestamp: number) => {
          this.displayNotification("A user has left the game.");
          console.log('User left the game.' + timestamp);
        }));
    });

    await this.gameHubService.connectToGame(`game:${this.roomId}`);

    if (this.isRoomAdmin) await this.gameHubService.startGame();
  }

  ngOnDestroy(): void {
    this.resetComponent;
    this.subscriptions.unsubscribe();
    this.gameHubService.stopConnection().then(() => {
      console.log('GameHub connection stopped.');
    });
  }

  private resetComponent = () => {
    this.winningTeam = '';
    this.drawEnabled = false;
    this.guessEnabled = false;
    this.blankCount = 0;
    this.msgNotificationsUI = [];
    this.selectables = [];
    this.hint = '';
    this.gameModelView = null;
    this.gameRoundView = null;
  }

  async selectWord(word: string) {
    if (this.autoCloseTimeout) {
      clearTimeout(this.autoCloseTimeout);
    }
    await this.gameHubService.selectWord(word);
    this.dialog.closeAll();
  }

  private displayNotification(message: string, timeoutInterval: number = 3000, roomState: string | null = null): void {
    this.notificationText = message;
    setTimeout(() => {
      if(this.gameEnded === true) {
        this.roomStateChange.emit(roomState!);
      }
      this.notificationText = '';
    }, timeoutInterval);
  }

  private prepareWordSelection(): void {
    if (this.wordPack && this.wordPack.length >= 4) {
      this.selectables = [];
      const shuffled = [...this.wordPack].sort(() => 0.5 - Math.random());
      this.selectables = shuffled.slice(0, 3);
    }
  }

  private showWordSelectModal(): void {
    if (this.selectables.length > 0) {
      this.dialog.open(this.wordSelectionModal, {
        data: { words: this.selectables },
        disableClose: true
      });
      this.autoCloseTimeout = setTimeout(async () => { 
        const randomWord = this.selectables[Math.floor(Math.random() * this.selectables.length)];
        await this.gameHubService.selectWord(randomWord);
        this.dialog.closeAll();
      }, 14500);
    }
  }
  private showPainterSelectOverlay(nickname: string): void {
    const overlay = document.createElement('div');
    overlay.className = 'painter-selecting-overlay';
    overlay.innerText = `${nickname} is selecting a word...`;
    document.querySelector('.game-container')!.appendChild(overlay);
  }

}
