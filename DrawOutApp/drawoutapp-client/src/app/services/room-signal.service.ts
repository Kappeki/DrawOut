import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { User } from '../models/user';
import { Room } from '../models/room';
import { __values } from 'tslib';

@Injectable({
  providedIn: 'root'
})
export class RoomSignalService {

  private readonly API_URL = 'https://localhost:7041';
  public hubConnection: HubConnection = new HubConnectionBuilder()
    .withUrl(`${this.API_URL}/roomhub`, { withCredentials: true }) // Connect to the backend hub
    .configureLogging(LogLevel.Information)
    .build();

  public messages$ = new BehaviorSubject<any>([]);
  public connectedUsers$ = new BehaviorSubject<User[]>([]);
  public connectedRoom$ = new BehaviorSubject<Room | null>(null);

  public messages: any[] = [];
  public users: User[] = [];
  public room: Room | null = null;

  constructor() {
    this.hubConnection?.on('ReceiveMessage', (sender: string, content: string, timestamp: string) => {
      this.messages = [...this.messages, { sender, content, timestamp }];
      this.messages$.next(this.messages);
    });
    this.hubConnection?.on('ConnectedUsers', (users: User[]) => {
      this.connectedUsers$.next(users);
    });
    this.hubConnection?.on('ConnectedRoom', (room: Room) => {
      this.connectedRoom$.next(room);
    });
  }

  public async startConnection() {
    // try{
    //   await this.hubConnection?.start();
    // }catch(err){
    //   console.error(err);
    // }
    await this.hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch(err => console.error('Error while starting connection: ' + err));
  }

  public async leaveRoom() {
    return await this.hubConnection?.stop().then(() => {
      this.connectedRoom$.next(null);
      this.messages$.next([]);
      this.connectedUsers$.next([]);
    }).catch(err => console.error('Error stopping connection:', err));
  }

  public async joinRoomById(roomId: string, password?: string) {
    return await this.hubConnection.invoke('JoinRoomById', roomId, password)
      .catch(err => console.error(err));
  }

  public async joinRoomByURL(roomURL: string, password?: string) {
    if (this.hubConnection?.state !== 'Connected') {
      console.error('Connection not established yet');
      return;
    }

    return this.hubConnection.invoke('JoinRoomByURL', roomURL, password)
      .catch(err => console.error(err));
  }

  public async sendMessageToRoom(roomId: string, message: string) {
    return await this.hubConnection?.invoke('SendMessageToRoom', roomId, message)
      .catch(err => console.error(err));
  }

  public onReceiveMessage(): void {
    this.hubConnection?.on('ReceiveMessage', (sender, content, timestamp) => {
      console.log(`Message from ${sender}: ${content} at ${timestamp}`);
    });
  }

  public onConnectedUsers(): void {
    this.hubConnection?.on('ConnectedUsers', (users) => {
      console.log('Connected users:', users);
    });
  }

  public onConnectedRoom(): void {
    this.hubConnection?.on('ConnectedRoom', (room) => {
      console.log('Connected room:', room);
    });
  }
}
