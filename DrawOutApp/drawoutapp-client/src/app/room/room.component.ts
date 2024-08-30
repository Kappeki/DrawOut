import { Component, HostListener, OnDestroy, OnInit } from '@angular/core';
import { DrawOutAPIService } from '../services/draw-out-api.service';
import { Room } from '../models/room';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { WhiteboardComponent } from '../whiteboard/whiteboard.component';
import { RoomSignalService } from '../services/room-signal.service';
import { User } from '../models/user';
import { ChatComponent } from '../chat/chat.component';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-room',
  standalone: true,
  imports: [CommonModule, WhiteboardComponent, ChatComponent, FormsModule],
  templateUrl: './room.component.html',
  styleUrl: './room.component.css'
})
export class RoomComponent implements OnInit{
  private roomId: string = '';
  private roomURL: string = '';

  room: Room | undefined;
  redTeam: string[] = [];
  blueTeam: string[] = [];
  chatMessages: any[] = [];
  users: User[] = [];
  chatInput: string = '';
  isRoomAdmin: boolean = false;
  gameStarted: boolean = false;
  currentRound: number = 1;

  constructor(
    private apiService: DrawOutAPIService, 
    private roomService: RoomSignalService, 
    private router: Router, 
    private route: ActivatedRoute,
  ) { }

  async ngOnInit(): Promise<void> { 
    await this.roomService.startConnection();

    this.roomService.connectedRoom$.subscribe(res => {
      this.room = res!;  
    });
    this.roomService.messages$.subscribe(res => {
      this.chatMessages = res;
    });
    this.roomService.connectedUsers$.subscribe(res => {
      this.users = res;
    });
    this.route.params.subscribe(params => {
      this.roomURL = params['roomURL'];
      this.roomService.joinRoomByURL(this.roomURL);
    });
  }

  @HostListener('window:beforeunload', ['$event'])
  @HostListener('window:popstate', ['$event'])
  handleWindowClose(event: any) {
    this.roomService.leaveRoom();
  }

  startGame() {}

  endGame() {}

  sendMessage(message: string) {
    this.roomService.sendMessageToRoom(this.roomURL, message);
  }
  
  copyInviteLink() {}

}
