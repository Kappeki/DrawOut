import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { MatSliderModule } from '@angular/material/slider';
import { MatPaginatorModule } from '@angular/material/paginator';
import { NgxSliderModule } from '@angular-slider/ngx-slider';
import { FabricModule  } from 'ngx-fabric-wrapper';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { FormsModule } from '@angular/forms';
import { HomeComponent } from './components/home/home.component';
import { FooterComponent } from './components/shared/footer/footer.component';
import { NavbarComponent } from './components/shared/navbar/navbar.component';
import { RoomListComponent } from './components/room-list/room-list.component';
import { RoomItemComponent } from './components/room-item/room-item.component';
import { RoomDetailComponent } from './components/room-detail/room-detail.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { TeamComponent } from './components/team/team.component';
import { UserComponent } from './components/user/user.component';
import { PaginatonComponent } from './components/paginaton/paginaton.component';
import { LayoutModule } from '@angular/cdk/layout';
import { DrawingComponent } from './components/drawing/drawing.component';
import { ChatComponent } from './components/chat/chat.component';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    FooterComponent,
    NavbarComponent,
    RoomListComponent,
    RoomItemComponent,
    RoomDetailComponent,
    TeamComponent,
    UserComponent,
    PaginatonComponent,
    DrawingComponent,
    ChatComponent
  ],
  imports: [
    BrowserModule, HttpClientModule,
    AppRoutingModule, FormsModule, BrowserAnimationsModule,
    MatSliderModule, LayoutModule, MatPaginatorModule,
    FabricModule
    //NgxSliderModule ne radi jer Anuglar Material ima problema sa Angular 17 verzijom i dual sliderom
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
