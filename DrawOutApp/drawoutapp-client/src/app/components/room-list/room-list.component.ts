import { Component } from '@angular/core';
import { Room, RoomListItem } from "../../models/room";
import { DrawOutAPIService } from '../../services/draw-out-api.service';
import { Router } from '@angular/router';
import { RoomItemComponent } from '../room-item/room-item.component';
import { CommonModule } from '@angular/common';
import { RoomSignalService } from '../../services/room-signal.service';


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
  standalone: true,
  imports: [RoomItemComponent, CommonModule],
  templateUrl: './room-list.component.html',
  styleUrls: ['./room-list.component.css']
})
export class RoomListComponent {

  rooms: RoomListItem[] = [];
  myRooms: RoomListItem[] = [];
  activeTab: string = 'available';

  isAscending: boolean = true;
  isProtected: boolean = false;

  constructor(private apiService: DrawOutAPIService, private router: Router) { }

  ngOnInit(): void {
    this.loadRooms();
  }

  selectTab(tab: string): void {
    this.activeTab = tab;
    if (tab === 'available') {
      this.loadRooms();
    } else if (tab === 'myrooms') {
      this.loadMyRooms();
    }
  }

  loadRooms(): void {
    this.apiService.getRooms(this.isAscending, this.isProtected).subscribe({
      next: (rooms: any) => {
        this.rooms = rooms;
        console.log('Rooms loaded successfully');
      },
      error: (error: any) => {
        console.error('Error loading rooms', error);
      },
      complete: () => {
        console.log('Room loading completed');
      }
    });
  }

  loadMyRooms(): void {
    this.apiService.getMyRooms().subscribe({
      next: (rooms: any) => {
        this.myRooms = rooms;
        console.log('My rooms loaded successfully');
      },
      error: (error: any) => {
        console.error('Error loading my rooms', error);
      },
      complete: () => {
        console.log('My room loading completed');
      }
    });
  }

  onRoomSelected(roomId: string): void {
    this.router.navigate(['/room/by-id', roomId]);
  }
}
