import { Component, OnInit} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DrawoutApiService } from '../../services/drawout-api.service';
// import { SignalrService } from '../services/signalr.service';

@Component({
  selector: 'app-room-detail',
  templateUrl: './room-detail.component.html',
  styleUrl: './room-detail.component.css'
})
export class RoomDetailComponent {

  roomId?: string; //IZMENA
  isRoomAdmin: boolean = true;
  mockRoom = {
    id: 'room123',
    name: 'Drawing Fun',
    users: [
      { id: 'user1', name: 'Alice', team: 'red' },
      { id: 'user2', name: 'Bob', team: 'blue' },
    ],
    admin: 'user1', // User ID of the room administrator
    isGameStarted: false,
    // ... other room properties ...
  };

  constructor(
    private route: ActivatedRoute,
    private drawoutApiService: DrawoutApiService
  ) {}

  // ngOnInit() {
  //   this.roomId = this.route.snapshot.paramMap.get('id'); //IZMENA
  //   if (this.roomId) {
  //     this.loadRoomDetails(this.roomId);
  //   } else {
  //     //mozda redirekcija
  //     console.log("No room ID provided.");
  //   }
  // }

  loadRoomDetails(roomId: string) {
    console.log(this.mockRoom);
  }

  joinTeam(teamColor: 'red' | 'blue') {
    // Logic to join a team
    // Call API to update the team of the current user in this room
  }
}
