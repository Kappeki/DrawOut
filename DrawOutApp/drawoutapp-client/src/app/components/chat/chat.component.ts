import { CommonModule } from '@angular/common';
import { Component, ElementRef, EventEmitter, Input, OnDestroy, Output, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.css'
})
export class ChatComponent implements OnDestroy {
  @Input() messages: any[] = [];
  @Input() chatInput: string = '';
  @Input() enableGuessing: boolean = false;
  @Output() messageSent = new EventEmitter<string>();
  @Output() guessSent = new EventEmitter<string>();
  @ViewChild('chatMessages') private chatMessagesContainer: ElementRef | undefined;  // To reference the message container
  charCount: number = 0;

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  ngOnDestroy(): void {
    this.messages = [];
    this.messageSent.complete();
    this.guessSent.complete();
  }

  onSendMessage() {
    if (this.chatInput.trim()) {
      if (this.enableGuessing) {
        this.guessSent.emit(this.chatInput);
      } else {
        this.messageSent.emit(this.chatInput);
      }
      this.chatInput = '';
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
    
    const charCountElement = document.querySelector('.char-count');
    
    if (charCountElement) {
      if (this.charCount > 0) {
        charCountElement.classList.add('visible');
        charCountElement.classList.remove('hidden');
      } else {
        charCountElement.classList.add('hidden');
        charCountElement.classList.remove('visible');
      }
    }
  }  

  scrollToBottom(): void {
    if (this.chatMessagesContainer) {
      try {
        this.chatMessagesContainer.nativeElement.scrollTop = this.chatMessagesContainer.nativeElement.scrollHeight;
      } catch (err) {
        console.error('Scrolling error:', err);
      }
    }
  }
}