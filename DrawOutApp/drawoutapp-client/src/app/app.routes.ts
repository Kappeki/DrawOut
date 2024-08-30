import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { NgModule } from '@angular/core';
import { RoomListComponent } from './room-list/room-list.component';
import { RoomComponent } from './room/room.component';

export const routes: Routes = [
    { path: '', component: HomeComponent },
    { path: 'rooms', component: RoomListComponent},
    { path: 'room/:roomURL', component: RoomComponent }
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})

export class AppRoutingModule { }
