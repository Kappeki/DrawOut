using DrawOutApp.Server.Models;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using System.Drawing;

namespace DrawOutApp.Server.Entities
{
    public enum ActionType { Start, Move, End }
    public enum ToolType { Brush, Fill, Eraser }
    public class DrawingAction
    {
        public string _id { get; set; } = null!;
        public string GameId { get; set; } = null!;
        public string StrokeId { get; set; } = null!;

        public ActionType ActionType { get; set; }
        public ToolType ToolType { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public string Color { get; set; } = "#FFFFFF";
        public int BrushSize { get; set; }
        public long Timestamp { get; set; } //used as score for the sorted set
        public string? PainterName { get; set; }
    }
}
