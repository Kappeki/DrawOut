import { Room } from './room.model';

export interface RoomListResponse {
    Rooms: Room[];       // Array of Room objects
    TotalRooms: number;  // Total number of Room objects
  }