import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RoomListItem } from '../shared/models/room';

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
}
