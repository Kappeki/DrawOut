import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app.routes';
import { AppComponent } from './app.component';
import { HomeComponent } from './components/home/home.component';
import { RoomListComponent } from './components/room-list/room-list.component';
import { RoomItemComponent } from './components/room-item/room-item.component';
import { CommonModule } from '@angular/common';
import { RoomComponent } from './components/room/room.component';
import { WhiteboardComponent } from './components/whiteboard/whiteboard.component';
import { FormsModule } from '@angular/forms';
import { RoomSettingsComponent } from './components/room-settings/room-settings.component';

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
    RoomSettingsComponent,
    FormsModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  providers: [],
  bootstrap: []
})
export class AppModule { }
