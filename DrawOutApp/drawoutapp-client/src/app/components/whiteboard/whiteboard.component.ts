import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { DrawingActionView } from '../../models/drawing-action';
import { v4 as uuidv4 } from 'uuid';
import { GameHubService } from '../../services/game-hub.service';
import { DrawingCareTaker, DrawingOriginator } from '../../models/drawing-memento';
import { FormsModule } from '@angular/forms';
import { SelectableDirective } from '../../directives/selectable.directive';
import { fromEvent, Subscription } from 'rxjs';
import { throttleTime } from 'rxjs/operators';


@Component({
  selector: 'app-whiteboard',
  standalone: true,
  imports: [CommonModule, FormsModule, SelectableDirective],
  templateUrl: './whiteboard.component.html',
  styleUrl: './whiteboard.component.css'
})
export class WhiteboardComponent implements OnInit, AfterViewInit {
  colors: string[] = [
    '#000000', '#FFFFFF', '#FF0000', '#00FF00', '#0000FF',
    '#FFFF00', '#00FFFF', '#FF00FF', '#C0C0C0', '#808080',
    '#800000', '#808000', '#008000', '#800080', '#008080',
    '#000080', '#FF6600', '#FFCC00', '#CCFF00', '#33FF66',
    '#66FF33', '#CC33FF', '#6633FF', '#3366FF', '#66CCFF',
    '#99FF33', '#FF33CC', '#FF3366', '#FF6633', '#FF9966'
  ];
  tools = ['Brush', 'Fill', 'Eraser'];
  brushSizes: number[] = [4, 8, 12, 16, 20];
  selectedColor: string = '#000000';
  selectedBrushSize: number = 8;
  selectedTool: string = 'Brush';

  @Input() drawEnabled: boolean = false;
  @Input() gameId: string = '';

  @ViewChild('canvas', { static: false }) canvas!: ElementRef<HTMLCanvasElement>;
  private ctx: CanvasRenderingContext2D | null = null;
  private actions: DrawingActionView[] = [];
  private isDrawing: boolean = false;
  private currentStrokeId: string | null = null;

  private originator!: DrawingOriginator;
  private caretaker: DrawingCareTaker = new DrawingCareTaker();

  private subscriptions: Subscription = new Subscription();

  constructor(private gameHubService: GameHubService) { }

  ngOnInit(): void {
  }

  ngAfterViewInit(): void {
    this.ctx = this.canvas.nativeElement.getContext('2d', { willReadFrequently: true }) as CanvasRenderingContext2D;
    this.originator = new DrawingOriginator(this.ctx!);
    this.setupCanvasEvents();
    this.setupListeners();

    fromEvent<MouseEvent>(this.canvas.nativeElement, 'mousemove')
      .pipe(throttleTime(20)) // Adjust the time as needed (in ms)
      .subscribe(event => this.onMouseMove(event));

  }

  private setupCanvasEvents(): void {
    const canvasElement = this.canvas.nativeElement;

    this.resizeCanvas();

    canvasElement.addEventListener('mousedown', this.onMouseDown.bind(this));
    canvasElement.addEventListener('mousemove', this.onMouseMove.bind(this));
    canvasElement.addEventListener('mouseup', this.onMouseUp.bind(this));
    canvasElement.addEventListener('mouseout', this.onMouseOut.bind(this));

    //window.addEventListener('resize', this.resizeCanvas.bind(this));
  }

  private resizeCanvas(): void {
    const canvasElement = this.canvas.nativeElement;
    canvasElement.width = canvasElement.offsetWidth;
    canvasElement.height = canvasElement.offsetHeight;
  }

  private pendingActions: DrawingActionView[] = [];
  private lastSendTime: number = 0;

  private onMouseDown(event: MouseEvent): void {
    if (!this.drawEnabled) return;
    this.isDrawing = true;
    this.currentStrokeId = uuidv4();

    this.caretaker.addMemento(this.originator.save());
    this.gameHubService.sendMementoSave();

    const rect = this.canvas.nativeElement.getBoundingClientRect();
    const scaleX = this.canvas.nativeElement.width / rect.width;
    const scaleY = this.canvas.nativeElement.height / rect.height;

    const startX = (event.clientX - rect.left) * scaleX;
    const startY = (event.clientY - rect.top) * scaleY;

    const startAction: DrawingActionView = {
      gameId: this.gameId,
      strokeId: this.currentStrokeId,
      actionType: 'Start',
      toolType: this.selectedTool,
      x: startX,
      y: startY,
      color: this.selectedColor,
      brushSize: this.selectedBrushSize,
      timestamp: Date.now()
    };

    this.pendingActions.push(startAction);

    this.ctx!.beginPath();
    this.ctx!.moveTo(startX, startY);
  }

  private onMouseMove(event: MouseEvent): void {
    if (!this.isDrawing) return;

    const rect = this.canvas.nativeElement.getBoundingClientRect();
    const scaleX = this.canvas.nativeElement.width / rect.width;
    const scaleY = this.canvas.nativeElement.height / rect.height;

    const currentX = (event.clientX - rect.left) * scaleX;
    const currentY = (event.clientY - rect.top) * scaleY;

    this.ctx!.lineTo(currentX, currentY);
    this.ctx!.strokeStyle = this.selectedColor;
    this.ctx!.lineWidth = this.selectedBrushSize;
    this.ctx!.lineCap = "round";
    this.ctx!.lineJoin = "round";
    this.ctx!.stroke();

    const action: DrawingActionView = {
      gameId: this.gameId,
      strokeId: this.currentStrokeId!,
      actionType: 'Move',
      toolType: this.selectedTool,
      x: currentX,
      y: currentY,
      color: this.selectedColor,
      brushSize: this.selectedBrushSize,
      timestamp: Date.now()
    };

    this.pendingActions.push(action);

    if (Date.now() - this.lastSendTime > 50) {

      this.sendPendingActions();
    }
  }

  private onMouseUp(event: MouseEvent): void {
    if (!this.isDrawing) return;
    this.isDrawing = false;
    this.ctx!.closePath();

    const rect = this.canvas.nativeElement.getBoundingClientRect();
    const scaleX = this.canvas.nativeElement.width / rect.width;
    const scaleY = this.canvas.nativeElement.height / rect.height;

    const endX = (event.clientX - rect.left) * scaleX;
    const endY = (event.clientY - rect.top) * scaleY;

    const endAction: DrawingActionView = {
      gameId: this.gameId,
      strokeId: this.currentStrokeId!,
      actionType: 'End',
      toolType: this.selectedTool,
      x: endX,
      y: endY,
      color: this.selectedColor,
      brushSize: this.selectedBrushSize,
      timestamp: Date.now()
    };

    this.pendingActions.push(endAction);

    // Send all pending actions immediately after mouse up
    this.sendPendingActions();

    this.currentStrokeId = null;
  }

  private onMouseOut(event: MouseEvent): void {
    if (!this.isDrawing) return;
    this.isDrawing = false;
    //this.ctx!.closePath();
    this.currentStrokeId = null;
  }


  private sendPendingActions(): void {
    if (this.pendingActions.length > 0) {
      this.actions = [...this.actions, ...this.pendingActions];
      this.gameHubService.sendDrawingAction(this.pendingActions);
      this.pendingActions = [];
      this.lastSendTime = Date.now();
    }
  }


  private applyDrawingAction(action: DrawingActionView): void {
    if (action.actionType === 'Start') {
      this.ctx!.beginPath();
      this.ctx!.moveTo(action.x, action.y);
    } else if (action.actionType === 'Move') {
      this.ctx!.lineTo(action.x, action.y);
      this.ctx!.strokeStyle = action.color!;
      this.ctx!.lineWidth = action.brushSize;
      this.ctx!.stroke();
    } else if (action.actionType === 'End') {
      this.ctx!.lineTo(action.x, action.y);
      this.ctx!.stroke();
      //this.ctx!.closePath();
    }
  }

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
    if (!this.drawEnabled) return;

    const previousState = this.caretaker.undo();
    if (previousState) {
      this.originator.restore(previousState);
    }

    const lastStroke = this.actions.pop();
    if (lastStroke) {
      this.gameHubService.undoAction(lastStroke.strokeId!);
    }
  }

  clearCanvas(): void {
    this.ctx!.clearRect(0, 0, this.canvas.nativeElement.width, this.canvas.nativeElement.height);
    this.actions = [];
    this.caretaker = new DrawingCareTaker();
    this.gameHubService.clearCanvas();
  }

  private setupListeners(): void {
    this.subscriptions.add(
      this.gameHubService.drawingActions$.subscribe(actions => {
        if (actions.length === 0) {
          this.ctx!.clearRect(0, 0, this.canvas.nativeElement.width, this.canvas.nativeElement.height);
          this.caretaker = new DrawingCareTaker();
          this.actions = [];
        } else {
          console.log('Applying NEW actions');
          actions.map(action => this.applyDrawingAction(action));
          this.actions = [...actions];
        }
      }));
    this.subscriptions.add(
      this.gameHubService.currentStroke$.subscribe((res: string) => {
        const previousState = this.caretaker.undo();
        if (previousState) {
          this.originator.restore(previousState);
        }
        this.actions = this.actions.filter(action => action.strokeId !== res);
      }));
    this.subscriptions.add(
      this.gameHubService.hubConnection.on('SaveMemento', (res: number) => {
        console.log('Memento saved at' + res);
        this.caretaker.addMemento(this.originator.save());
      }));
  }

  private restoreCanvas(actions: DrawingActionView[]): void {
    this.ctx!.clearRect(0, 0, this.canvas.nativeElement.width, this.canvas.nativeElement.height);
    //actions.forEach(action => this.applyDrawingAction(action));
    actions.map(action => this.applyDrawingAction(action));
  }

}
