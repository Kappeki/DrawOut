using DrawOutApp.Server.Entities;
using StackExchange.Redis;

namespace DrawOutApp.Server.Repositories.Contracts
{
    public interface IGameRepo
    {
        ITransaction BeginTransaction();
        Task<bool> SaveGameAsync(Game game, TimeSpan? expiry);
        Task<Game?> GetGameAsync(string gameId);
        Task DeleteGameAsync(string gameId);
        Task<T?> GetFromHashSet<T>(string setKey, string valueKey);
    }
}
