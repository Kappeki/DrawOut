using DrawOutApp.Server.Entities;

namespace DrawOutApp.Server.Repositories.Contracts
{
    public interface IRoundRepo
    {
        //Task AddRoundAsync(Round round);
        Task<Round?> GetRoundAsync(string roundId);
        Task UpdateRoundAsync(string roundId, Round round);
        Task DeleteRoundAsync(string roundId);
        Task<List<DrawingAction>> GetDrawingActionsForRoundAsync(string roundId);  
        Task AddDrawingActionToRoundAsync(string roundId, DrawingAction drawingAction);
    }
}
