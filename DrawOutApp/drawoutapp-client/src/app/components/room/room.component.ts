import { Component, HostListener, OnDestroy, OnInit } from '@angular/core';
import { DrawOutAPIService } from '../../services/draw-out-api.service';
import { Room } from '../../models/room';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RoomHubService } from '../../services/room-hub.service';
import { User } from '../../models/user';
import { FormsModule } from '@angular/forms';
import { ChatComponent } from '../chat/chat.component';
import { WhiteboardComponent } from '../whiteboard/whiteboard.component';
import { BehaviorSubject, Subscription } from 'rxjs';
import UserListComponent from "../user-list/user-list.component";
import { RoomSettingsComponent } from '../room-settings/room-settings.component';
import { SessionService } from '../../services/session.service';
import { GameComponent } from '../game/game.component';
import { GameModelView } from '../../models/game';

@Component({
  selector: 'app-room',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ChatComponent,
    WhiteboardComponent, UserListComponent, RoomSettingsComponent,
    GameComponent
  ],
  templateUrl: './room.component.html',
  styleUrl: './room.component.css'
})
export class RoomComponent implements OnInit, OnDestroy {

  private roomURL: string = '';
  private subscriptions: Subscription = new Subscription();
  private latestGuessSubject = new BehaviorSubject<string | null>(null);

  room: Room | null = null;
  redTeam: string[] = [];
  blueTeam: string[] = [];
  chatMessages: any[] = [];
  currentRound: number = 1;
  totalRounds: number = 8;

  currentTeam: string | null = null;
  chatInput: string = '';

  //game important properties
  availableWords: string[] = [];
  users: User[] = [];
  isRoomAdmin: boolean = false;
  roomId: string = '';
  enableGuessing = true;

  //imati u vidu da NECE game da se pokrene ako admin ne udje iz room liste
  //trebalo bi i ovo da se ispravi kasnije


  constructor(
    private roomHubService: RoomHubService,
    private router: Router,
    private route: ActivatedRoute,
    private sessionService: SessionService,
    private apiService: DrawOutAPIService
  ) { }

  async ngOnInit(): Promise<void> {
    await this.roomHubService.startConnection().then(() => {

      this.subscriptions.add(
        this.roomHubService.connectedRoom$.subscribe(res => {
          this.room = res!;
          this.roomURL = this.room?.roomURL!;
          const sessionId = this.sessionService.getSessionId();
          this.isRoomAdmin = this.room && this.room?.roomAdminId === sessionId;
        }));

      this.subscriptions.add(
        this.roomHubService.messages$.subscribe(res => {
          this.chatMessages = res;
        }));

      this.subscriptions.add(
        this.roomHubService.connectedUsers$.subscribe(res => {
          this.users = res;
          this.redTeam = [];
          this.blueTeam = [];
          this.users.forEach(user => {
            if (user.roles?.includes('Red')) {
              this.redTeam.push(user.nickname);
            } else if (user.roles?.includes('Blue')) {
              this.blueTeam.push(user.nickname);
            }
          });
        }));

      this.subscriptions.add(
        this.roomHubService.teams$.subscribe(res => {
          const userIndex = this.users.findIndex(user => user.nickname === res.nickname);
          if (userIndex !== -1) {
            const user = this.users[userIndex];
            user.roles = user.roles!.filter(role => role !== 'Red' && role !== 'Blue');
            user.roles.push(res.newTeam);
            this.users[userIndex] = user;
          }
          if (res.oldTeam) {
            if (res.oldTeam === 'Red' && res.newTeam === 'Blue') {
              this.redTeam = this.redTeam.filter(name => name !== res.nickname);
              this.blueTeam.push(res.nickname);
            } else if (res.oldTeam === 'Blue' && res.newTeam === 'Red') {
              this.blueTeam = this.blueTeam.filter(name => name !== res.nickname);
              this.redTeam.push(res.nickname);
            }
          } else {
            if (res.newTeam === 'Red') {
              this.redTeam.push(res.nickname);
            } else if (res.newTeam === 'Blue') {
              this.blueTeam.push(res.nickname);
            }
          }
        }));

      this.subscriptions.add(
        this.roomHubService.roomSettings$.subscribe(setting => {
          if (setting && this.room) {
            switch (setting.settingName) {
              case 'RoundTime':
                this.room.roundTime = setting.settingValue;
                break;
              case 'SelectedWordPack':
                this.room.selectedWordPack = setting.settingValue;
                this.getWordsFromPack();
                break;
              case 'CustomWords':
                this.room.customWords = setting.settingValue.split(',');
                break;
            }
          }
        }));
    });

    this.subscriptions.add(
      this.route.paramMap.subscribe(params => {
        if (this.route.snapshot.url[1].path === 'by-id') {
          this.roomId = params.get('roomId')!;
          this.roomHubService.joinRoomById(this.roomId);
        } else if (this.route.snapshot.url[1].path === 'by-url') {
          this.roomURL = params.get('roomURL')!;
          this.roomHubService.joinRoomByURL(this.roomURL);
        }
      }));

  }

  async ngOnDestroy(): Promise<void> {
    this.subscriptions.unsubscribe();
    await this.handleWindowClose(null);
  }

  @HostListener('window:beforeunload', ['$event'])
  @HostListener('window:popstate', ['$event'])
  async handleWindowClose(event: any) {
    this.chatMessages = [];
    this.room = null;
    this.users = [];
    this.redTeam = [];
    this.blueTeam = [];
    this.currentTeam = null;
    this.roomId = '';
    await this.roomHubService.leaveRoom();
  }

  getWordsFromPack(): void {
    this.apiService.getWordsByPackName(this.room?.selectedWordPack!).subscribe(words => {
      this.availableWords = words;
    });
  }

  startGame() {
    if (this.isRoomAdmin) {
      this.roomHubService.notifyGameStart(this.roomURL);
      this.apiService.startGame(this.roomId).subscribe(res => {
        console.log(res);
      });
    }
  }

  copyInviteLink() { }

  sendMessage(message: string) {
    this.roomHubService.sendMessageToRoom(this.roomURL, message);
    if (this.enableGuessing && this.room?.roomState === 'InGame') {
      this.latestGuessSubject.next(message);
    }
  }

  get latestGuess$() {
    return this.latestGuessSubject.asObservable();
  }

  handleRoundChange(event: { newRound: number, totalRounds: number }): void {
    this.currentRound = event.newRound;
    this.totalRounds = event.totalRounds;
  }

  handleTeamJoin(event: { oldTeam: string | null, newTeam: string }) {
    this.currentTeam = event.newTeam;
    this.roomHubService.switchTeam(this.roomURL, event.oldTeam, event.newTeam);
  }

  handleSettingChange(event: { settingName: string, settingValue: any }) {
    if (this.isRoomAdmin && this.room) {
      this.roomHubService.changeRoomSettings(this.roomURL, event.settingName, event.settingValue);
    }
  }

  handleGuessEnabled(event: boolean) {
    this.enableGuessing = event;
  }

  handleChatClear(event: string[]) {
    this.chatMessages = event;
  }

}
