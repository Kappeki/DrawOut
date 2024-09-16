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
import { GameComponent } from './components/game/game.component';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule, MatLabel } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatOptionModule } from '@angular/material/core';
import { SelectableDirective } from './directives/selectable.directive';
import { MatChipsModule } from '@angular/material/chips';
import { MatIcon, MatIconModule } from '@angular/material/icon';

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
    GameComponent,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatSelectModule,
    MatLabel,
    MatOptionModule,
    SelectableDirective,
    MatChipsModule,
    MatIconModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  providers: [],
  bootstrap: []
})
export class AppModule { }
