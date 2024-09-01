import { CommonModule } from '@angular/common';
import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { WhiteboardComponent } from '../whiteboard/whiteboard.component';
import { GameSignalService } from '../../services/game-signal.service';
import { Game } from '../../models/game';

@Component({
  selector: 'app-game',
  standalone: true,
  imports: [CommonModule, FormsModule, WhiteboardComponent],
  templateUrl: './game.component.html',
  styleUrl: './game.component.css'
})
export class GameComponent implements OnInit {

  game: Game | undefined;

  @Output() roundChange = new EventEmitter<{ newRound: number, totalRounds: number }>();

  constructor(private gameService: GameSignalService) {}

  ngOnInit(): void {
    this.gameService.game$.subscribe((game: Game) => {
      this.game = game;
    });

    // setInterval(() => {
    //   if (this.game.mainTimer > 0) {
    //     this.game.mainTimer--;
    //   } else if (this.game.mainTimer === 0 && this.game.stealTimer > 0) {
    //     this.game.stealTimer--;
    //   } else if (this.game.stealTimer === 0) {
    //     this.endRound();
    //   }
    // }, 1000);
  }

  onCorrectGuess(team: 'blue' | 'red') {
    this.gameService.handleCorrectGuess(team);
  }

  endRound() {
    // Logic to end the current round and potentially start the next round
    this.gameService.startNewRound();
  }
}
