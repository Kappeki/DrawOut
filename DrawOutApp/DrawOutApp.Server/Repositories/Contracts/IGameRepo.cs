using DrawOutApp.Server.Entities;
using StackExchange.Redis;

namespace DrawOutApp.Server.Repositories.Contracts
{
    public interface IGameRepo
    {
        ITransaction BeginTransaction();
        Task<Game?> GetGameAsync(string gameSessionId);
        Task AddGameAsync(Game game, TimeSpan? expiry = null);
        Task AddGameAsync(Game game, ITransaction tran, TimeSpan? expiry = null);
        //Task<bool> UpdateGameAsync(string gameSessionId, Game game);
        Task DeleteGameAsync(string gameSessionId);
        Task<T?> GetFromHashSet<T>(string setKey, string valueKey);
    }
}
