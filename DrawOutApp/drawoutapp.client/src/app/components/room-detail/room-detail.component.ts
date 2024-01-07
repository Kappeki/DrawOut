import { Component, OnInit} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DrawoutApiService } from '../../services/drawout-api.service';

@Component({
  selector: 'app-room-detail',
  templateUrl: './room-detail.component.html',
  styleUrl: './room-detail.component.css'
})
export class RoomDetailComponent {

  constructor(private route: ActivatedRoute) {}

  ngOnInit() {
  this.route.params.subscribe(params => {
    const roomId = params['id'];
    // Use the roomId to load room details from your service
    });
  }

  // loadRoomDetails(roomId: string) {
  //   this.drawoutApiService.getRoomDetails(roomId).subscribe(roomDetails => {
  //     // Do something with the room details
  //   },
  //   error => {
  //     console.log(error);
  //   });
  // }
}
