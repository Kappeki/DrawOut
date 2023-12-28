using DrawOutApp.Server.Entities;

namespace DrawOutApp.Server.Repositories.Contracts
{
    public interface IGameRepo
    {
        Task<Game?> GetGameAsync(string gameSessionId);
        Task AddGameAsync(Game game);
        Task UpdateGameAsync(string gameSessionId, Game game);
        Task DeleteGameAsync(string gameSessionId);
    }
}
