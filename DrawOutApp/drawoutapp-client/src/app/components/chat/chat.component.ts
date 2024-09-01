import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnDestroy, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.css'
})
export class ChatComponent {
  @Input() messages: any[] = [];
  @Input() chatInput: string = '';
  @Output() messageSent = new EventEmitter<string>();
  charCount: number = 0;

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

  formatTimestamp(timestamp: number): string {
    const date = new Date(timestamp * 1000); // Assuming timestamp is in seconds
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false }); // You can customize the format as needed
  }

  updateCharacterCount(): void {
    this.charCount = this.chatInput.length;
  }
}
