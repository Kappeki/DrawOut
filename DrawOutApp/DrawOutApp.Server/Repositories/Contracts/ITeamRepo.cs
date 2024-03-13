using DrawOutApp.Server.Entities;
using StackExchange.Redis;

namespace DrawOutApp.Server.Repositories.Contracts
{
    public interface ITeamRepo
    {
        ITransaction BeginTransaction();
        Task<Team?> GetTeamAsync(string key);
        Task AddOrUpdateTeamAsync(Team team, TimeSpan? expiry = null);
        Task AddOrUpdateTeamAsync(Team team, ITransaction tran, TimeSpan? expiry = null);
        Task AddTeammateAsync(string cacheKey, string teammateId);
        Task RemoveTeammateAsync(string cacheKey, string teammateId);
        Task UpdateScoreAsync(string cacheKey, int score);
        Task UpdateTeamLeaderAsync(string cacheKey, string teamLeaderId);
        Task DeleteTeamAsync(string key);
    }
}
