import { Component } from '@angular/core';
import { Room, RoomListItem } from "../../models/room";
import { DrawOutAPIService } from '../../services/draw-out-api.service';
import { Router } from '@angular/router';
import { RoomItemComponent } from '../room-item/room-item.component';
import { CommonModule } from '@angular/common';
import { query } from '@angular/animations';

@Component({
  selector: 'app-room-list',
  standalone: true,
  imports: [RoomItemComponent, CommonModule],
  templateUrl: './room-list.component.html',
  styleUrls: ['./room-list.component.css', '../../app.component.css']
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
    const snackbar = document.getElementById('snackbar');
    this.apiService.getRooms(this.isAscending, this.isProtected).subscribe({
      next: (rooms: any) => {
        this.rooms = rooms;
        console.log('Rooms loaded successfully');
      },
      error: (error: any) => {
        snackbar!.innerText = 'No rooms available!';
        snackbar!.className = "show";

        setTimeout(() => {
          snackbar!.className = snackbar!.className.replace("show", "");
        }, 3000);

        this.rooms = [];
        // console.error('Error loading rooms', error);
      },
      complete: () => {
        console.log('Room loading completed');
      }
    });
  }

  loadMyRooms(): void {
    const snackbar = document.getElementById('snackbar');
    this.apiService.getMyRooms(this.isAscending, this.isProtected).subscribe({
      next: (rooms: any) => {
        this.myRooms = rooms;
        console.log('My rooms loaded successfully');
      },
      error: (error: any) => {
        snackbar!.innerText = 'No rooms available!';
        snackbar!.className = "show";

        setTimeout(() => {
          snackbar!.className = snackbar!.className.replace("show", "");
        }, 3000);

        this.myRooms = [];
        // console.error('Error loading my rooms', error);
      },
      complete: () => {
        console.log('My room loading completed');
      }
    });
  }

  toggleAscending(): void {
    this.isAscending = !this.isAscending;
    if(this.activeTab === 'available') this.loadRooms();
    if(this.activeTab === 'myrooms') this.loadMyRooms();
  }

  toggleProtected(): void {
    this.isProtected = !this.isProtected;
    if(this.activeTab === 'available') this.loadRooms();
    if(this.activeTab === 'myrooms') this.loadMyRooms();
  }

  onRoomSelected(roomItem: RoomListItem): void {
    this.router.navigate(['/room/by-id', roomItem.roomId], { queryParams: { passwd: roomItem.hasPassword } });
  }
}
