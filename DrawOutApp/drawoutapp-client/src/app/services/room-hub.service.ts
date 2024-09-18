import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { User } from '../models/user';
import { Room } from '../models/room';
import { __values } from 'tslib';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class RoomHubService {

  private readonly API_URL = 'https://localhost:7041';
  public hubConnection: HubConnection = new HubConnectionBuilder()
    .withUrl(`${this.API_URL}/roomhub`, { withCredentials: true })
    .configureLogging(LogLevel.Information)
    .build();

  public messages$ = new BehaviorSubject<any>([]);
  public connectedUsers$ = new BehaviorSubject<User[]>([]);
  public connectedRoom$ = new BehaviorSubject<Room | null>(null);
  public teams$ = new BehaviorSubject<any>([]);
  public roomSettings$ = new BehaviorSubject<{ settingName: string, settingValue: any } | null>(null);

  public messages: any[] = [];
  public users: User[] = [];
  public room: Room | null = null;
  public teams: any[] = [];

  constructor() {
    this.setupListeners();
  }

  public async startConnection() {
    await this.hubConnection
      .start()
      .then(() => {
        console.log('Connection started');
      })
      .catch(err => console.error('Error while starting connection: ' + err));
  }

  public async leaveRoom() {
    return await this.hubConnection?.stop().then(() => {
      this.messages = [];
      this.connectedRoom$.next(null);
      this.messages$.next([]);
      this.connectedUsers$.next([]);
      this.teams$.next([]);
    }).catch(err => console.error('Error stopping connection:', err));
  }

  public async joinRoomById(roomId: string, password?: string, router?: Router) {
    return await this.hubConnection.invoke('JoinRoomById', roomId, password)
      .catch(err => {
        router?.navigate(['/']);
        alert("Wrong password!");
      });
  }

  public async joinRoomByURL(roomURL: string) {
    if (this.hubConnection?.state !== 'Connected') {
      console.error('Connection not established yet');
      return;
    }

    return this.hubConnection.invoke('JoinRoomByURL', roomURL)
      .catch(err => console.error(err));
  }

  public async sendMessageToRoom(roomId: string, message: string) {
    return await this.hubConnection?.invoke('SendMessageToRoom', roomId, message)
      .catch(err => console.error(err));
  }

  public async switchTeam(roomURL: string, oldTeam: string | null, newTeam: string) {
    return await this.hubConnection?.invoke('SwitchTeam', roomURL, oldTeam, newTeam)
      .catch(err => console.error(err));
  }

  private handleTeamSwitch(oldTeam: string | null, newTeam: string, nickname: string): void {
    if (oldTeam === 'Red') {
      this.teams = this.teams.filter(member => !(member.teamName === 'Red' && member.nickname === nickname));
    } else if (oldTeam === 'Blue') {
      this.teams = this.teams.filter(member => !(member.teamName === 'Blue' && member.nickname === nickname));
    }
    this.teams$.next({ oldTeam, newTeam, nickname });
  }

  public async changeRoomSettings(roomURL: string, settingName: string, settingValue: any) {
    return await this.hubConnection?.invoke('ChangeRoomSettings', roomURL, settingName, settingValue)
      .catch(err => console.error(err));
  }

  public async updateRoomState(roomURL: string, state: string) {
    return await this.hubConnection?.invoke('UpdateRoomState', roomURL, state)
      .catch(err => console.error(err));
  }

  private setupListeners(): void {
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
    this.hubConnection?.on('ReceiveTeamSwitch', (oldTeam: string | null, newTeam: string, nickname: string) => {
      this.handleTeamSwitch(oldTeam, newTeam, nickname);
    });
    this.hubConnection?.on('RoomSettingsChanged', (settingName: string, settingValue: any) => {
      this.roomSettings$.next({ settingName, settingValue });
    });
  }
}
