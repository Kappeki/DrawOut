import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app.routes';
import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { RoomListComponent } from './room-list/room-list.component';
import { RoomItemComponent } from './room-item/room-item.component';
import { CommonModule } from '@angular/common';
import { RoomComponent } from './room/room.component';
import { WhiteboardComponent } from './whiteboard/whiteboard.component';
import { ChatComponent } from './chat/chat.component';

@NgModule({
  declarations: [
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HomeComponent,
    AppComponent,
    RoomListComponent,
    RoomItemComponent,
    RoomComponent,
    CommonModule,
    WhiteboardComponent,
    ChatComponent
  ],
  schemas: [ CUSTOM_ELEMENTS_SCHEMA],
  providers: [],
  bootstrap: []
})
export class AppModule { }
