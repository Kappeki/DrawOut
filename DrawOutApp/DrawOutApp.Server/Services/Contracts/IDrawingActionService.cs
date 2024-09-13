using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Services.Contracts
{
    public interface IDrawingActionService
    {
        Task AddDrawingActionAsync(DrawingActionModel model);
        Task<List<DrawingActionModel>> GetDrawingActionsSinceAsync(string roomId, long sinceTimestamp, string painterName);
        Task<List<DrawingActionModel>> GetLastNActionsAsync(string roomId, int n, string painterName);
        Task ClearDrawingActionsAsync(string roomId, string painterName);
        Task UndoStrokeAsync(string roomId, string painterName, string strokeId);
    }
}
