import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { RoomListComponent } from './room-list/room-list.component';
import { RoomItemComponent } from './room-item/room-item.component';

@NgModule({
  declarations: [
    RoomListComponent,
    RoomItemComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    AppComponent,
    HomeComponent
  ],
  providers: [],
  bootstrap: []
})
export class AppModule { }
