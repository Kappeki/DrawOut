import { Component, HostListener, OnDestroy, OnInit } from '@angular/core';
import { DrawOutAPIService } from '../../services/draw-out-api.service';
import { Room } from '../../models/room';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RoomSignalService } from '../../services/room-signal.service';
import { User } from '../../models/user';
import { FormsModule } from '@angular/forms';
import { ChatComponent } from '../chat/chat.component';
import { WhiteboardComponent } from '../whiteboard/whiteboard.component';
import { Subscription } from 'rxjs';
import UserListComponent from "../user-list/user-list.component";
import { RoomSettingsComponent } from '../room-settings/room-settings.component';

@Component({
  selector: 'app-room',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ChatComponent, 
    WhiteboardComponent, UserListComponent, RoomSettingsComponent
  ],
  templateUrl: './room.component.html',
  styleUrl: './room.component.css'
})
export class RoomComponent implements OnInit, OnDestroy {

  private roomId: string = '';
  private roomURL: string = '';
  private subscriptions: Subscription = new Subscription();

  room: Room | null = null;
  redTeam: string[] = [];
  blueTeam: string[] = [];
  chatMessages: any[] = [];
  users: User[] = [];
  isRoomAdmin: boolean = false;
  gameStarted: boolean = false;
  currentRound: number = 1;

  currentTeam: string | null = null;
  chatInput: string = '';

  constructor(
    private apiService: DrawOutAPIService,
    private roomService: RoomSignalService,
    private router: Router,
    private route: ActivatedRoute,
  ) { }

  async ngOnInit(): Promise<void> {

    await this.roomService.startConnection();

    this.subscriptions.add(this.roomService.connectedRoom$.subscribe(res => {
      this.room = res!;
      this.roomURL = this.room?.roomURL!;
    }));

    this.subscriptions.add(this.roomService.messages$.subscribe(res => {
      this.chatMessages = res;
    }));

    this.subscriptions.add(this.roomService.connectedUsers$.subscribe(res => {
      this.users = res;
    }));

    this.subscriptions.add(this.roomService.teams$.subscribe(res => {
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

    this.subscriptions.add(this.route.paramMap.subscribe(params => {
      if (this.route.snapshot.url[1].path === 'by-id') {
        this.roomId = params.get('roomId')!;
        this.roomService.joinRoomById(this.roomId);
      } else if (this.route.snapshot.url[1].path === 'by-url') {
        this.roomURL = params.get('roomURL')!;
        this.roomService.joinRoomByURL(this.roomURL);
      }
    }));
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
    this.handleWindowClose(null);
  }

  @HostListener('window:beforeunload', ['$event'])
  @HostListener('window:popstate', ['$event'])
  handleWindowClose(event: any) {
    this.chatMessages = [];
    this.room = null;
    this.users = [];
    this.redTeam = [];
    this.blueTeam = [];
    this.currentTeam = null;
    this.roomService.leaveRoom();
  }

  startGame() { }

  endGame() { }

  sendMessage(message: string) {
    this.roomService.sendMessageToRoom(this.roomURL, message);
  }

  copyInviteLink() { }

  handleTeamJoin(event: { oldTeam: string | null, newTeam: string }) {
    this.currentTeam = event.newTeam;
    this.roomService.switchTeam(this.roomURL, event.oldTeam, event.newTeam);
  }
}
