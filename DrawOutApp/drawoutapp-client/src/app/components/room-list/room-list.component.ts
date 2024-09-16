import { Component } from '@angular/core';
import { Room, RoomListItem } from "../../models/room";
import { DrawOutAPIService } from '../../services/draw-out-api.service';
import { Router } from '@angular/router';
import { RoomItemComponent } from '../room-item/room-item.component';
import { CommonModule } from '@angular/common';

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

  //fale filteri

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
