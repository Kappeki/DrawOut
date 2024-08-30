import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule, NgModel } from '@angular/forms';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.css'
})
export class ChatComponent {
  @Input() messages: any[] = [];
  @Input() chatInput: string = '';
  @Output() messageSent = new EventEmitter<string>();

  onSendMessage() {
    if (this.chatInput.trim()) {
      this.messageSent.emit(this.chatInput);
      this.chatInput = ''; // Clear the input after sending the message
    }
  }

  handleKeyDown(event: KeyboardEvent) {
    if (event.key === 'Enter') {
      this.onSendMessage();
    }
  }
}
