using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Services.Contracts
{
    public interface IGameService
    {
        Task<Result<GameModel,string>> CreateGameAsync(string roomId);
        Task<Result<GameModel?,string>> GetGameAsync(string gameSessionId);
        Task <Result<bool,string>> UpdateGameAsync(string gameSessionId, GameModel gameModel);
        Task DeleteGameAsync(string gameSessionId);
    }
}
