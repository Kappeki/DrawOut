import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { NgModule } from '@angular/core';
import { RoomListComponent } from './components/room-list/room-list.component';
import { RoomComponent } from './components/room/room.component';

export const routes: Routes = [
    { path: '', component: HomeComponent },
    { path: 'rooms', component: RoomListComponent },
    { path: 'room/by-id/:roomId', component: RoomComponent },
    { path: 'room/by-url/:roomURL', component: RoomComponent }
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})

export class AppRoutingModule { }
