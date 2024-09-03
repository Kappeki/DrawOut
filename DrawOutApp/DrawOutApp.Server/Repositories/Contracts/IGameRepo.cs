using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;
using StackExchange.Redis;

namespace DrawOutApp.Server.Repositories.Contracts
{
    public interface IGameRepo
    {
        Task<string> SaveGameAsync(Game game, TimeSpan? expiry = null);
        Task<Game?> GetGameAsync(string gameId);
        Task<bool> UpdateGameRoundAsync(GameRound gameRound, TimeSpan? expiry = null);
        Task<int> IncrementScoreAsync(string gameId, string teamName, int incrementValue);
        Task DecrementTimerAsync(string gameId, string timerName, int decrementValue);
        Task<bool> UpdateSelectedWordAsync(string gameId, string selectedWord);
        Task DeleteGameAsync(string gameId);
        Task<T?> GetFromHashSet<T>(string setKey, string valueKey);
    }
}
