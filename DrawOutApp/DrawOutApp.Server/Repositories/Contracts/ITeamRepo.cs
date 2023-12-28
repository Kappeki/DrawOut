using DrawOutApp.Server.Entities;

namespace DrawOutApp.Server.Repositories.Contracts
{
    public interface ITeamRepo
    {
        Task<Team?> GetTeamAsync(string teamId);
        Task AddTeamAsync(Team team);
        Task UpdateTeamAsync(string teamId, Team team);
        Task DeleteTeamAsync(string teamId);
    }
}
