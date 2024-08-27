namespace DrawOutApp.Server.Models
{
    public enum ToolType
    {
        Brush,
        FillBucket
    }

    public enum ActionType
    {
        Draw,
        Fill,
        Undo,
        Clear
    }

    public class DrawingActionModel
    {
        public ToolType Tool { get; set; }
        public ActionType Action { get; set; }
        public String Color { get; set; }
        public int BrushSize { get; set; }
        public List<(int X, int Y)> StrokePath { get; set; }
        public bool IsFilled { get; set; }
        public bool ClearCanvas { get; set; }

        public DrawingActionModel()
        {
            StrokePath = new List<(int X, int Y)>();
            Color = "#000000";
        }

        // Constructor and any necessary methods would go here
    }
}
