import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Room, RoomListItem } from '../models/room';
import { GameModelView } from '../models/game';

@Injectable({
  providedIn: 'root'
})
export class DrawOutAPIService {

  private readonly API_URL = 'https://localhost:7041';
  constructor(private http: HttpClient) { }

  createUser(userPrefs: any): Observable<any> {
    return this.http.post(`${this.API_URL}/User/CreateUser`, userPrefs, { withCredentials: true });
  }

  getSession(): Observable<any> {
    return this.http.get(`${this.API_URL}/User/GetSession`, { withCredentials: true });
  }

  updateUserPrefs(userPrefs: any): Observable<any> {
    return this.http.put(`${this.API_URL}/User/UpdateUser`, userPrefs, { withCredentials: true, responseType: 'text' });
  }

  getRooms(isAscending?: boolean, isProtected?: boolean): Observable<RoomListItem[]> {
    let params = new HttpParams();

    if (isAscending !== undefined) {
      params = params.append('isAscending', isAscending.toString());
    }
    if (isProtected !== undefined) {
      params = params.append('isProtected', isProtected.toString());
    }

    return this.http.get<RoomListItem[]>(`${this.API_URL}/Room/allRooms`, { params, withCredentials: true });
  }

  getMyRooms(): Observable<RoomListItem[]> {
    return this.http.get<RoomListItem[]>(`${this.API_URL}/Room/myRooms`, { withCredentials: true });
  }

  createRoom(roomName: string, password?: string): Observable<string> {
    const payload = { roomName, password };
    return this.http.post<string>(`${this.API_URL}/Room/CreateRoom`, payload, { withCredentials: true });
  }

  getRoomByUrl(roomId: string): Observable<Room> {
    return this.http.get<Room>(`${this.API_URL}/Room/${roomId}/get`, { withCredentials: true, responseType: 'text' as 'json' },);
  }

  updateRoom(room: Room): Observable<any> {
    return this.http.put(`${this.API_URL}/Room/update`, room, { withCredentials: true });
  }

  getAllWordPacks(): Observable<string[]> {
    return this.http.get<string[]>(`${this.API_URL}/Room/GetAllWordPacks`, { withCredentials: true });
  }
  getWordsByPackName(packName: string): Observable<string[]> {
    const params = new HttpParams().set('packName', packName);
    return this.http.get<string[]>(`${this.API_URL}/Room/GetWordsByPackName`, { params });
  }

  startGame(roomId: string): Observable<GameModelView> {
    const params = new HttpParams().set('roomId', roomId);
    return this.http.get<GameModelView>(`${this.API_URL}/Room/StartGame`, { params });
  }

}
