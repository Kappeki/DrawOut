import { CommonModule } from '@angular/common';
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Room } from '../../models/room';
import { DrawOutAPIService } from '../../services/draw-out-api.service';
import { debounceTime, Subject } from 'rxjs';

@Component({
  selector: 'app-room-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './room-settings.component.html',
  styleUrl: './room-settings.component.css'
})
export class RoomSettingsComponent {
  @Input() room: Room | null = null;
  @Input() isRoomAdmin: boolean = false;
  @Output() settingChanged = new EventEmitter<{ settingName: string, settingValue: any }>();

  drawTime: number = 60;
  wordPack: string = 'default';
  customWords: string = '';
  useCustomWords: boolean = false;

  wordPacks: string[] = [];

  private customWordsSubject = new Subject<string>();

  constructor(private apiService: DrawOutAPIService) { }

  ngOnInit(): void {
    if (this.room) {
      this.drawTime = this.room.roundTime;
      this.wordPack = this.room.selectedWordPack || 'default';
      this.customWords = this.room.customWords?.join(', ') || '';
      this.useCustomWords = !!this.room.customWords?.length;
    }
    this.apiService.getAllWordPacks().subscribe((packs) => {
      this.wordPacks = packs;
    });

    this.customWordsSubject.pipe(
      debounceTime(300)
    ).subscribe((newCustomWords) => {
      this.updateCustomWords(newCustomWords);
    });
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

  onCustomWordsChanged(newCustomWords: string) {
    if (this.isRoomAdmin && this.room) {
      this.customWordsSubject.next(newCustomWords);  // Emit the value to the Subject
    }
  }

  private updateCustomWords(newCustomWords: string) {
    this.customWords = newCustomWords;
    this.settingChanged.emit({ settingName: 'CustomWords', settingValue: newCustomWords });
  }
}
