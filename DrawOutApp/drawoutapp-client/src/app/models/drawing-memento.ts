export class DrawingMemento {
    constructor(public readonly state: ImageData) { }
}

export class DrawingOriginator {
    private ctx: CanvasRenderingContext2D;

    constructor(ctx: CanvasRenderingContext2D) {
        this.ctx = ctx;
    }

    save(): DrawingMemento {
        return new DrawingMemento(this.ctx.getImageData(0, 0, this.ctx.canvas.width, this.ctx.canvas.height));
    }

    restore(memento: DrawingMemento): void {
        this.ctx.putImageData(memento.state, 0, 0);
    }
}

export class DrawingCareTaker {
    private mementos: DrawingMemento[] = [];

    addMemento(memento: DrawingMemento): void {
        this.mementos.push(memento);
    }

    undo(): DrawingMemento | null {
        if (this.mementos.length > 0) {
            return this.mementos.pop()!;
        }
        return null;
    }
}