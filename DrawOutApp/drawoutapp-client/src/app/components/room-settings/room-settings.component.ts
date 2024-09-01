import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Room } from '../../models/room';
import { DrawOutAPIService } from '../../services/draw-out-api.service';

@Component({
  selector: 'app-room-settings',
  standalone: true,
  imports: [CommonModule, FormsModule,],
  templateUrl: './room-settings.component.html',
  styleUrl: './room-settings.component.css'
})
export class RoomSettingsComponent {
  @Input() room: Room | null = null;
  @Input() currentUser: any;
  drawTime: number = 60;
  wordPack: string = 'default';
  customWords: string = '';
  useCustomWords: boolean = false;

  constructor(private apiService: DrawOutAPIService) {
    
  }

  getCustomWordsArray(): string[] {
    return this.customWords.split(',').map(word => word.trim()).filter(word => word.length > 0);
  }

  // get isRoomAdmin(): boolean {
  //   return this.room && this.room.roomAdminId === this.currentUser.id;
  // }

  updateDrawTime(newDrawTime: number) {
    if (this.room) {
      this.room.roundTime = newDrawTime;
      this.updateRoom();
    }
  }

  updateWordPack(newWordPack: string) {
    if (this.room) {
      this.room.selectedWordPack = newWordPack;
      this.updateRoom();
    }
  }

  updateCustomWords(newCustomWords: string[]) {
    if (this.room) {
      this.room.customWords = newCustomWords;
      this.updateRoom();
    }
  }

  updateRoom() {
    if (this.room) {
      this.apiService.updateRoom(this.room).subscribe(
        response => console.log('Room updated successfully'),
        error => console.error('Error updating room:', error)
      );
    }
  }
}
