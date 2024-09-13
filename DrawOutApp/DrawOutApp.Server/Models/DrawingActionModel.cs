namespace DrawOutApp.Server.Models
{

    public class DrawingActionModel
    {
        public string? GameId { get; set; }
        public string? StrokeId { get; set; }
        
        public string? ActionType { get; set; }
        public string? ToolType { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public string? Color { get; set; }
        public int BrushSize { get; set; }
        public long Timestamp { get; set; }
        public string? PainterName { get; set; }
    }
}
