using DrawOutApp.Server.Models;
using MongoDB.Bson.IO;
using Newtonsoft.Json;

namespace DrawOutApp.Server.Entities
{
    public class DrawingAction
    {
        public string Tool { get; set; }
        public string Action { get; set; }
        public String Color { get; set; }
        public int BrushSize { get; set; }
        public string StrokePathSerialized { get; set; } 
        public bool IsFilled { get; set; }
        public bool ClearCanvas { get; set; }

        public DrawingAction() { }

        public DrawingAction(DrawingActionModel action)
        {
            Tool = action.Tool.ToString();
            Action = action.Action.ToString();
            Color = action.Color;
            BrushSize = action.BrushSize;
            StrokePathSerialized = Newtonsoft.Json.JsonConvert.SerializeObject(action.StrokePath);
            IsFilled = action.IsFilled;
            ClearCanvas = action.ClearCanvas;
        }

        public DrawingActionModel ToBusinessModel()
        {
            return new DrawingActionModel
            {
                Tool = Enum.Parse<ToolType>(this.Tool),
                Action = Enum.Parse<ActionType>(this.Action),
                Color = this.Color,
                BrushSize = this.BrushSize,
                StrokePath = Newtonsoft.Json.JsonConvert.DeserializeObject<List<(int X, int Y)>>(StrokePathSerialized),
                IsFilled = this.IsFilled,
                ClearCanvas = this.ClearCanvas
            };
        }
    }
}
