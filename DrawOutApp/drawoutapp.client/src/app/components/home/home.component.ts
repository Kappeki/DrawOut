import { Component } from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  nickname: string = '';
  selectedIcon: string = '';

  constructor() {}

  playGame(): void {
    // Implement what happens when the user clicks the play button
    console.log(`Nickname: ${this.nickname}, Selected Icon: ${this.selectedIcon}`);
    // You might want to call a service method here to handle the user joining the game
  }
}
