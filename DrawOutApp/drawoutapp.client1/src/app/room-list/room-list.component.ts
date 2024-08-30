import { Component } from '@angular/core';
import { Room, RoomListItem } from "../shared/models/room";
import { DrawOutAPIService } from '../services/draw-out-api.service';
import { Router } from '@angular/router';


export const MOCK_ROOMS: RoomListItem[] = [
  {
    roomId: '1',
    roomName: 'Fun Room',
    hasPassword: false,
    gameState: 'Waiting',
    playerCount: 4,
  },
  {
    roomId: '2',
    roomName: 'Private Room',
    hasPassword: true,
    gameState: 'InGame',
    playerCount: 6,
  },
  {
    roomId: '3',
    roomName: 'Open Lobby',
    hasPassword: false,
    gameState: 'Waiting',
    playerCount: 2,
  },
  {
    roomId: '4',
    roomName: 'Competitive Arena',
    hasPassword: true,
    gameState: 'InGame',
    playerCount: 8,
  },
  {
    roomId: '5',
    roomName: 'Casual Chat',
    hasPassword: false,
    gameState: 'Waiting',
    playerCount: 5,
  },
];

@Component({
  selector: 'app-room-list',
  templateUrl: './room-list.component.html',
  styleUrl: './room-list.component.css'
})
export class RoomListComponent {

rooms: RoomListItem[] = MOCK_ROOMS;

constructor(private apiService: DrawOutAPIService, private router: Router) { }

ngOnInit(): void {
  this.loadRooms();
}

loadRooms(): void {
  this.apiService.getRooms(true, false).subscribe((rooms) => {
    this.rooms = rooms;
  });
}

onRoomSelected(roomId: string): void {
  this.router.navigate(['/rooms', roomId]);
}

}
