import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Room } from '../models/room.model';
import { RoomListResponse } from '../models/room-list-response.model';

export const API_URL = 'http://localhost:5213'; //unencrypted, for encrypted use https://localhost:7041

@Injectable({
  providedIn: 'root'
})

export class DrawoutApiService {

  // private mockRooms: Room[]  = [
  //   { id: '1', name: 'Room Alpha', isLocked: false, playerCount: 3, timeLeft: 86400000 * 3 }, // 3 days left
  //   { id: '2', name: 'Room Beta', isLocked: true, password: '1234', playerCount: 5, timeLeft: 86400000 * 2 }, // 2 days left
  //   { id: '3', name: 'Room Gamma', isLocked: false, playerCount: 2, timeLeft: 86400000 * 4 }, // 4 days left
  //   { id: '4', name: 'Room Delta', isLocked: true, password: 'pass', playerCount: 7, timeLeft: 86400000 }, // 1 day left
  //   // ... more rooms ...
  //   { id: '20', name: 'Room Tesseract', isLocked: false, playerCount: 4, timeLeft: 86400000 * 5 } // 5 days left
  // ];
  
  constructor(private http:HttpClient ) { }

  getRandomNickname(): Observable<any> {
    return this.http.get(`${API_URL}/icons/random`);
  }

  getRandomIcon(): Observable<any> {
    return this.http.get(`${API_URL}/icons/random`);
  }

  getNextIcon(currentIconId: string): Observable<any> {
    return this.http.get(`${API_URL}/icons/next/${currentIconId}`);
  }

  // getRooms() : Observable<Room[]> {
  //   return this.http.get<Room[]>(`${API_URL}/rooms`);
  // }

  // getRooms(page: number = 1, pageSize: number = 15, filters?: any, sort?: string): Room[] {
  //   // Add logic to filter, sort, and paginate rooms
  //   return this.mockRooms.slice((page - 1) * pageSize, page * pageSize);
  // }

  getFilteredRooms(filterParams: any, page: number, pageSize: number = 15): Observable<RoomListResponse> {
    filterParams.page = page;
    filterParams.pageSize = pageSize;
    const quertParams = new URLSearchParams(filterParams as any).toString();
    return this.http.get<RoomListResponse>(`${API_URL}/rooms?${quertParams}`);
  }

}
