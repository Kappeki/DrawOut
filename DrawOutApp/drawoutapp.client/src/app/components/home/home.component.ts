import { Component, OnInit } from '@angular/core';
import { DrawoutApiService } from '../../services/drawout-api.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  currentNickname: string = '';
  currentIcon: string = '';

  constructor(private drawoutApiService: DrawoutApiService) {}

  ngOnInit(): void {
    this.drawoutApiService.getRandomIcon().subscribe(icon => {
      this.currentIcon = icon;
    });

    // this.drawoutApiService.getRandomNickname().subscribe(nickname => {
    //   this.currentNickname = nickname;
    // });
  }

  randomizeNickname(): void {
    this.drawoutApiService.getRandomNickname().subscribe(nickname => {
      this.currentNickname = nickname;
    });
  }

  randomizeIcon(): void {
    this.drawoutApiService.getRandomIcon().subscribe(icon => {
      this.currentIcon = icon;
    });
  }

  nextIcon(): void {
    this.drawoutApiService.getNextIcon(this.currentIcon).subscribe(icon => {
      this.currentIcon = icon;
    });
  }

  playGame(): void {
    // Implement what happens when the user clicks the play button
    console.log(`Nickname: ${this.currentNickname}, Selected Icon: ${this.currentIcon}`);
    // You might want to call a service method here to handle the user joining the game
  }

  createRoom() {
    // Implement what happens when the user clicks the create room button
    console.log('Create room');
    // You might want to call a service method here to handle the user creating a room
  }

  joinRandomRoom() {
    // Implement what happens when the user clicks the join random room button
    console.log('Join random room');
    // You might want to call a service method here to handle the user joining a random room
  }
}
