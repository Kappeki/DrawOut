import { NgModule } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { FormsModule } from '@angular/forms';
import { HomeComponent } from './home/home.component';
import { RoomListComponent } from './room-list/room-list.component';
import { RoomItemComponent } from './room-item/room-item.component';

@NgModule({
  declarations: [
    RoomListComponent,
    RoomItemComponent // napraviti preko komande ng generate component home i app component
  ],
  imports: [
    AppRoutingModule, FormsModule
  ],
  providers: [],
  bootstrap: []
})
export class AppModule { }