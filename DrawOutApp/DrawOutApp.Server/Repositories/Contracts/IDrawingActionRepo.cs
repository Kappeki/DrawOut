using DrawOutApp.Server.Entities;

namespace DrawOutApp.Server.Repositories.Contracts
{
    public interface IDrawingActionRepo
    {
        Task AddDrawingActionAsync(DrawingAction action);
        Task<List<DrawingAction>> GetDrawingActionsSinceAsync(string roomId, string painterName, long sinceTimestamp);
        Task<List<DrawingAction>> GetLastNActionsAsync(string roomId, string painterName, int n);
        Task ClearDrawingActionsAsync(string roomId, string painterName);
        Task UndoStrokeAsync(string roomId, string painterName, string strokeId);
    }
}
