import { Component, OnInit } from '@angular/core';
import { DrawoutApiService } from '../../services/drawout-api.service';
import { Room } from '../../models/room.model';
import { RoomListResponse } from '../../models/room-list-response.model';
import { MatPaginator, PageEvent } from '@angular/material/paginator';

@Component({
  selector: 'app-room-list',
  templateUrl: './room-list.component.html',
  styleUrl: './room-list.component.css'
})

export class RoomListComponent implements OnInit{
  rooms: Room[] = []; //za room model
  totalRooms: number = 0;
  currentPage: number = 0; //kao u nizu 0 je prvi element
  pageSize: number = 15;

  playerFilter = { min: 1, max: 8 };
  timeFilter = { min: 0, max: 5 };
  lockedFilter = false;
  searchTerm = '';
  sortOption = 'nameAs';

  constructor(private drawoutApiService : DrawoutApiService) {}

  ngOnInit(): void {
    this.updateFilters();
  }

  onJoinRoom(room: Room) {
    // Logic to join the room
  }

  // loadRooms() {
  //   this.rooms = this.drawoutApiService.getRooms(this.currentPage, this.pageSize);
  // }

  updateFilters() {
    const filterParams = {
      playerMin: this.playerFilter.min,
      playerMax: this.playerFilter.max,
      timeMin: this.timeFilter.min,
      timeMax: this.timeFilter.max,
      locked: this.lockedFilter,
      search: this.searchTerm,
      sort: this.sortOption
    };

    console.log(filterParams);

    this.drawoutApiService.getFilteredRooms(filterParams, this.currentPage * this.pageSize, this.pageSize).subscribe(
      (response: RoomListResponse) => {
        this.rooms = response.Rooms;
        this.totalRooms = response.TotalRooms;
      },
      (error) => {
        // Handle errors here
      }
    );
  }

  handlePageEvent(event: PageEvent) {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.updateFilters();
  }

  // goToPage(page: number) {
  //   this.currentPage = page;
  //   this.updateFilters();
  // }
}
