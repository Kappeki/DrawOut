import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})

export class SignalrService {

  private hubConnection!: signalR.HubConnection;

  public startConnection(): Promise<void> {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/chatHub') // Your SignalR hub URL
      .build();

    return this.hubConnection
      .start()
      .then(() => console.log('SignalR connection established.'))
      .catch(err => console.error('Error starting SignalR connection:', err));
  }

  public joinRoom(roomId: string): void {
    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('AddToGroup', roomId)
        .catch(err => console.error('Error joining room:', err));
    }
  }

  public leaveRoom(roomId: string): void {
    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('RemoveFromGroup', roomId)
        .catch(err => console.error('Error leaving room:', err));
    }
  }

  public sendMessage(roomId: string, message: string): void {
    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('SendMessageToGroup', roomId, message)
        .catch(err => console.error('Error sending message:', err));
    }
  }

  public onMessageReceived(subscribeToNewMessage: (message: string) => void): void {
    this.hubConnection.on('ReceiveMessage', (message) => {
      subscribeToNewMessage(message);
    });
  }

  public disconnect(): Promise<void> {
    return this.hubConnection.stop();
  }
}
