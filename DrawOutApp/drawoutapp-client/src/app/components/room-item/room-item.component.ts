import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { RoomListItem } from '../../models/room';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-room-item',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './room-item.component.html',
  styleUrls: ['./room-item.component.css']
})
export class RoomItemComponent implements OnInit {
  @Input() room!: RoomListItem;
  @Output() roomSelected = new EventEmitter<RoomListItem>();

  constructor() { }

  ngOnInit(): void { }

  onJoinRoom(): void {
    this.roomSelected.emit(this.room);
  }

}
