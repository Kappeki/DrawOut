import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-whiteboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './whiteboard.component.html',
  styleUrl: './whiteboard.component.css'
})
export class WhiteboardComponent {
  colors: string[] = [
    '#000000', '#FFFFFF', '#FF0000', '#00FF00', '#0000FF',
    '#FFFF00', '#00FFFF', '#FF00FF', '#C0C0C0', '#808080',
    '#800000', '#808000', '#008000', '#800080', '#008080',
    '#000080', '#FF6600', '#FFCC00', '#CCFF00', '#33FF66',
    '#66FF33', '#CC33FF', '#6633FF', '#3366FF', '#66CCFF',
    '#99FF33', '#FF33CC', '#FF3366', '#FF6633', '#FF9966'
  ];
  isRoomAdmin: boolean = false;
  gameStarted: boolean = false;
  currentRound: number = 1;
  brushSizes: number[] = [4, 8, 12, 16, 20];
  selectedColor: string = '#000000';
  selectedBrushSize: number = 8;
  selectedTool: string = 'brush';

  selectColor(color: string): void {
    this.selectedColor = color;
  }

  selectBrushSize(size: number): void {
    this.selectedBrushSize = size;
  }

  selectTool(tool: string): void {
    this.selectedTool = tool;
  }

  undoLastStroke(): void {
    // Logic to undo the last stroke
  }

  clearCanvas(): void {
    // Logic to clear the canvas
  }

  startGame() { }

}
