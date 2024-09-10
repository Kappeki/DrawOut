import { Injectable } from '@angular/core';
import { GameModelView, GameRoundView } from '../models/game';
import { BehaviorSubject } from 'rxjs';

import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private gameModel: GameModelView | null = null;
  private gameRound: GameRoundView | null = null;

  private gameSubject = new BehaviorSubject<GameModelView | null>(null);
  public game$ = this.gameSubject.asObservable();

  // private consolidateGame() {
  //   if (this.gameModel && this.gameRound) {
  //     const game: Game = {
  //       _id: this.gameModel._id,
  //       roomId: this.gameModel.roomId,
  //       teamLeaders: this.gameModel.teamLeaders,
  //       painterOrder: this.gameModel.painterOrder,
  //       totalRounds: this.gameModel.totalRounds,
  //       blueScore: this.gameRound.blueScore,
  //       redScore: this.gameRound.redScore,
  //       currentRound: this.gameRound.currentRound,
  //       currentPainter: this.gameRound.currentPainter,
  //       selectedWord: this.gameRound.selectedWord,
  //       mainTimer: this.gameRound.mainTimer,
  //       stealTimer: this.gameRound.stealTimer,
  //     }
  //     this.gameSubject.next(game);
  //   }
  // }

  // public updateGameModel(gameModel: GameModel) {
  //   this.gameModel = gameModel;
  //   this.consolidateGame();
  // }

  // public updateGameRound(gameRound: GameRound) {
  //   this.gameRound = gameRound;
  //   this.consolidateGame();
  // }

  // public updateGame(game: Game) {
  //   this.gameSubject.next(game);
  // }


  private determinePainterOrder(users: User[]): string[] {
    const redTeam = users.filter(user => user.roles!.includes('Red'));
    const blueTeam = users.filter(user => user.roles!.includes('Blue'));

    const combinedOrder: string[] = [];
    const firstTeam = Math.random() < 0.5 ? redTeam : blueTeam;
    const secondTeam = firstTeam === redTeam ? blueTeam : redTeam;

    let i = 0;
    let j = 0;

    while (i < firstTeam.length || j < secondTeam.length) {
      if (i < firstTeam.length) {
        combinedOrder.push(firstTeam[i]._sessionKey);
        i++;
      }
      if (j < secondTeam.length) {
        combinedOrder.push(secondTeam[j]._sessionKey);
        j++;
      }
    }

    return combinedOrder;
  }

  private selectTeamLeaders(users: User[]): { [team: string]: string } {
    const redTeam = users.filter(user => user.roles!.includes('Red'));
    const blueTeam = users.filter(user => user.roles!.includes('Blue'));

    const redLeader = redTeam[Math.floor(Math.random() * redTeam.length)];
    const blueLeader = blueTeam[Math.floor(Math.random() * blueTeam.length)];

    return {
      Red: redLeader._sessionKey,
      Blue: blueLeader._sessionKey,
    };
  }
}

