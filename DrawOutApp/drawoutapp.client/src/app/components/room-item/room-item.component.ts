import { Component } from '@angular/core';
import { Input, Output, EventEmitter } from '@angular/core';
import { Room } from '../../models/room.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-room-item',
  templateUrl: './room-item.component.html',
  styleUrl: './room-item.component.css'
})
export class RoomItemComponent {
  @Input() room!: Room;
  @Output() join = new EventEmitter<Room>();

  constructor(private router: Router) {}

  joinRoom(roomId: string) {
    this.router.navigate(['/room', roomId]);
  }
}
