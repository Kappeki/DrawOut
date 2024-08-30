import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Room, RoomListItem } from '../shared/models/room';

@Component({
  selector: 'app-room-item',
  templateUrl: './room-item.component.html',
  styleUrl: './room-item.component.css'
})
export class RoomItemComponent {
  @Input() room!: RoomListItem;
  @Output() roomSelected = new EventEmitter<string>(); // Output event to notify parent component when a room is selected

  constructor() {}

  onJoinRoom(): void {
    this.roomSelected.emit(this.room.roomId); // Emit the roomId when the user selects this room
  }

}
