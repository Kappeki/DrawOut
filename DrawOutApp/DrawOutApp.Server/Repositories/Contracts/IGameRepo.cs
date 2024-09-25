using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;
using StackExchange.Redis;

namespace DrawOutApp.Server.Repositories.Contracts
{
    public interface IGameRepo
    {
        Task<string> SaveGameAsync(Game game, TimeSpan? expiry = null);
        Task<Game?> GetGameAsync(string gameId);
        Task<Game?> UpdateGameRoundAsync(GameRoundModel gameRound, TimeSpan? expiry = null);
        Task<int> IncrementScoreAsync(string gameId, string teamName, int incrementValue);
        Task DeleteGameAsync(string gameId);
    }
}
