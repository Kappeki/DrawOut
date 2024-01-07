// chat.component.ts
import { Component, Input, OnInit } from '@angular/core';
import { SignalrService } from '../../services/signalr.service';

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit {
  @Input() roomId?: string;
  messages: string[] = ["Hello", "World", "Proba", "Proba2"];
  newMessage: string = '';

  constructor(private signalrService: SignalrService) {}

  ngOnInit(): void {
    if (this.roomId) { // Check if roomId is defined
      this.signalrService.startConnection().then(() => {
        this.signalrService.joinRoom(this.roomId!);
        this.signalrService.onMessageReceived((message) => {
          this.messages.push(message);
        });
      });
    }
  }

  sendMessage(): void {
    if (this.newMessage.trim() && this.roomId) { // Check if roomId is defined
      this.signalrService.sendMessage(this.roomId, this.newMessage);
      this.newMessage = '';
    }
  }

  ngOnDestroy(): void {
    if (this.roomId) { // Check if roomId is defined
      this.signalrService.leaveRoom(this.roomId);
    }
  }
}
