import { CommonModule } from '@angular/common';
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Room } from '../../models/room';
import { DrawOutAPIService } from '../../services/draw-out-api.service';

@Component({
  selector: 'app-room-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './room-settings.component.html',
  styleUrl: './room-settings.component.css'
})
// @Input() room: Room | null = null;
// @Input() currentUser: any;
// drawTime: number = 60;
// wordPack: string = 'default';
// customWords: string = '';
// useCustomWords: boolean = false;

// constructor(private apiService: DrawOutAPIService) {

// }

// getCustomWordsArray(): string[] {
//   return this.customWords.split(',').map(word => word.trim()).filter(word => word.length > 0);
// }

// // get isRoomAdmin(): boolean {
// //   return this.room && this.room.roomAdminId === this.currentUser.id;
// // }

// updateDrawTime(newDrawTime: number) {
//   if (this.room) {
//     this.room.roundTime = newDrawTime;
//     this.updateRoom();
//   }
// }

// updateWordPack(newWordPack: string) {
//   if (this.room) {
//     this.room.selectedWordPack = newWordPack;
//     this.updateRoom();
//   }
// }

// updateCustomWords(newCustomWords: string[]) {
//   if (this.room) {
//     this.room.customWords = newCustomWords;
//     this.updateRoom();
//   }
// }

// updateRoom() {
//   if (this.room) {
//     this.apiService.updateRoom(this.room).subscribe(
//       response => console.log('Room updated successfully'),
//       error => console.error('Error updating room:', error)
//     );
//   }
// }
export class RoomSettingsComponent {
  @Input() room: Room | null = null;
  @Input() isRoomAdmin: boolean = false;  // Determine if the current user is an admin
  @Output() settingChanged = new EventEmitter<{ settingName: string, settingValue: any }>();

  // Bind directly to the room properties
  drawTime: number = 60;
  wordPack: string = 'default';
  customWords: string = '';
  useCustomWords: boolean = false;

  constructor(private apiService: DrawOutAPIService) { }

  ngOnInit(): void {
    if (this.room) {
      this.drawTime = this.room.roundTime;
      this.wordPack = this.room.selectedWordPack || 'default';
      this.customWords = this.room.customWords?.join(', ') || '';
      this.useCustomWords = !!this.room.customWords?.length;
    }
  }

  updateDrawTime(newDrawTime: number) {
    if (this.isRoomAdmin && this.room) {
      this.drawTime = newDrawTime;
      this.settingChanged.emit({ settingName: 'RoundTime', settingValue: newDrawTime });
    }
  }

  updateWordPack(newWordPack: string) {
    if (this.isRoomAdmin && this.room) {
      this.wordPack = newWordPack;
      this.settingChanged.emit({ settingName: 'SelectedWordPack', settingValue: newWordPack });
    }
  }

  //treba nesto posebno za custom words da se stavi
  updateCustomWords(newCustomWords: string) {
    if (this.isRoomAdmin && this.room) {
      const customWordsArray = newCustomWords.split(',').map(word => word.trim());
      this.customWords = newCustomWords;
      this.settingChanged.emit({ settingName: 'CustomWords', settingValue: customWordsArray });
    }
  }

}
