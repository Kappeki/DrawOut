export interface DrawingActionView {
    gameId?: string;
    strokeId?: string;
    actionType?: string;
    toolType?: string;
    x: number;
    y: number;
    color?: string;
    brushSize: number;
    timestamp: number;
    painterName?: string;
}