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

@Component({
  selector: 'app-room',
  standalone: true,
  imports: [CommonModule, FormsModule, ChatComponent, WhiteboardComponent],
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
    this.roomService.leaveRoom();
  }

  startGame() { }

  endGame() { }

  sendMessage(message: string) {
    this.roomService.sendMessageToRoom(this.roomURL, message);
  }

  copyInviteLink() { }

}
