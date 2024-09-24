import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ElementRef, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
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
export class WhiteboardComponent implements OnInit, AfterViewInit, OnDestroy {
  colors: string[] = [
    '#ffffff', '#c1c1c1', '#ef130b', '#ff7100', '#ffe400',
    '#00cc00', '#00ff91', '#00b2ff', '#231fd3', '#a300ba',
    '#df69a7', '#ffac8e', '#a0522d', 
    '#000000', '#505050', '#740b07', '#c23800', '#e8a200', 
    '#004619', '#00785d', '#00569e', '#0e0865', '#550069', 
    '#873554', '#cc774d', '#63300d'
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

  private pendingActions: DrawingActionView[] = [];
  private lastSendTime: number = 0;

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

  ngOnDestroy(): void {
    this.resetComponent;
    this.subscriptions.unsubscribe();
  }

  private resetComponent = () => {
    this.ctx!.clearRect(0, 0, this.canvas.nativeElement.width, this.canvas.nativeElement.height);
    this.actions = [];
    this.pendingActions = [];
    this.lastSendTime = 0;
    this.caretaker = new DrawingCareTaker();
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

  private onMouseDown(event: MouseEvent): void {
    if (!this.drawEnabled) return;
    this.currentStrokeId = uuidv4();

    this.caretaker.addMemento(this.originator.save());
    this.gameHubService.sendMementoSave();

    const rect = this.canvas.nativeElement.getBoundingClientRect();
    const scaleX = this.canvas.nativeElement.width / rect.width;
    const scaleY = this.canvas.nativeElement.height / rect.height;

    const startX = (event.clientX - rect.left) * scaleX;
    const startY = (event.clientY - rect.top) * scaleY;

    if (this.selectedTool === 'Fill') {

      this.fillCanvas(Math.floor(startX), Math.floor(startY), this.selectedColor);

      const fillAction: DrawingActionView = {
        gameId: this.gameId,
        strokeId: this.currentStrokeId,
        actionType: 'Bucket',
        toolType: 'Fill',
        x: startX,
        y: startY,
        color: this.selectedColor,
        brushSize: 0,
        timestamp: Date.now()
      };

      this.pendingActions.push(fillAction);
      this.sendPendingActions();

    } else {

      this.isDrawing = true;

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
  }

  private onMouseMove(event: MouseEvent): void {
    if (!this.isDrawing) return;

    const rect = this.canvas.nativeElement.getBoundingClientRect();
    const scaleX = this.canvas.nativeElement.width / rect.width;
    const scaleY = this.canvas.nativeElement.height / rect.height;

    const currentX = (event.clientX - rect.left) * scaleX;
    const currentY = (event.clientY - rect.top) * scaleY;

    this.ctx!.lineTo(currentX, currentY);
    if (this.selectedTool === 'Eraser') {
      this.ctx!.strokeStyle = '#FFFFFF';
    } else {
      this.ctx!.strokeStyle = this.selectedColor;
    }
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

    this.sendPendingActions();

    this.currentStrokeId = null;
  }

  private onMouseOut(event: MouseEvent): void {
    if (!this.isDrawing) return;
    this.isDrawing = false;
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

    // const rect = this.canvas.nativeElement.getBoundingClientRect();
    // const scaleX = this.canvas.nativeElement.width / rect.width;
    // const scaleY = this.canvas.nativeElement.height / rect.height;

    // const currentX = (event.clientX - rect.left) * scaleX;
    // const currentY = (event.clientY - rect.top) * scaleY;

    if (action.actionType === 'Bucket') {
      this.fillCanvas(Math.floor(action.x), Math.floor(action.y), action.color!);
    } else if (action.actionType === 'Start') {
      this.ctx!.beginPath();
      this.ctx!.moveTo(action.x, action.y);
    } else if (action.actionType === 'Move') {
      if (action.toolType === 'Eraser') {
        this.ctx!.strokeStyle = '#FFFFFF';
      }
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
    this.canvas.nativeElement.style.cursor = `url(/${tool.toLowerCase()}.png) 0 30, auto`;
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

  private fillCanvas(x: number, y: number, fillColor: string): void {
    const ctx = this.ctx!;
    const canvas = this.canvas.nativeElement;
    const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
    const data = imageData.data;

    const getColorAtPixel = (x: number, y: number) => {
      const offset = (y * canvas.width + x) * 4;
      return {
        r: data[offset],
        g: data[offset + 1],
        b: data[offset + 2],
        a: data[offset + 3]
      };
    };

    const setColorAtPixel = (x: number, y: number, color: { r: number, g: number, b: number, a: number }) => {
      const offset = (y * canvas.width + x) * 4;
      data[offset] = color.r;
      data[offset + 1] = color.g;
      data[offset + 2] = color.b;
      data[offset + 3] = color.a;
    };

    const targetColor = getColorAtPixel(x, y);
    const fillColorObj = {
      r: parseInt(fillColor.slice(1, 3), 16),
      g: parseInt(fillColor.slice(3, 5), 16),
      b: parseInt(fillColor.slice(5, 7), 16),
      a: 255 // Assuming fully opaque
    };

    if (targetColor.r === fillColorObj.r &&
      targetColor.g === fillColorObj.g &&
      targetColor.b === fillColorObj.b &&
      targetColor.a === fillColorObj.a) {
      return;
    }

    const pixelStack = [{ x, y }];

    while (pixelStack.length) {
      const newPos = pixelStack.pop();
      if (!newPos) continue;
      const { x, y } = newPos;

      let reachLeft = false;
      let reachRight = false;

      let currentY = y;
      let currentX = x;

      while (currentY >= 0 && this.isSameColor(getColorAtPixel(currentX, currentY), targetColor)) {
        currentY--;
      }

      currentY++;

      while (currentY < canvas.height && this.isSameColor(getColorAtPixel(currentX, currentY), targetColor)) {
        setColorAtPixel(currentX, currentY, fillColorObj);

        if (currentX > 0) {
          if (this.isSameColor(getColorAtPixel(currentX - 1, currentY), targetColor)) {
            if (!reachLeft) {
              pixelStack.push({ x: currentX - 1, y: currentY });
              reachLeft = true;
            }
          } else if (reachLeft) {
            reachLeft = false;
          }
        }

        if (currentX < canvas.width - 1) {
          if (this.isSameColor(getColorAtPixel(currentX + 1, currentY), targetColor)) {
            if (!reachRight) {
              pixelStack.push({ x: currentX + 1, y: currentY });
              reachRight = true;
            }
          } else if (reachRight) {
            reachRight = false;
          }
        }

        currentY++;
      }
    }

    ctx.putImageData(imageData, 0, 0);
  }

  private isSameColor(c1: any, c2: any): boolean {
    return c1.r === c2.r && c1.g === c2.g && c1.b === c2.b && c1.a === c2.a;
  }

}
