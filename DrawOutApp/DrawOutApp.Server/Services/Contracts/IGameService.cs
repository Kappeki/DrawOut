using DrawOutApp.Server.Hubs;
using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Services.Contracts
{
    public interface IGameService
    {
        Task StartGameAsync(string gameId, GameTimers gameTimers);
        Task SelectWordAsync(string gameId, string word, GameTimers gameTimers);
        Task SubmitGuessAsync(string userId, string gameId, string guess, GameTimers gameTimers);
        Task<GameRoundModel> CreateGameAsync(GameModel gameModel, List<string> userIds);
        Task<Result<int, string>> IncrementScoreAsync(string gameId, string teamName, int incrementValue);
        Task<bool> CheckGuessAsync(string gameId, string guess);
        Task DeleteGameAsync(string gameSessionId);
    }
}
