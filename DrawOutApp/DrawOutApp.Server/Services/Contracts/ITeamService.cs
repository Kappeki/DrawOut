using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Services.Contracts
{
    public interface ITeamService
    {
        Task<TeamModel> CreateTeamAsync();
        Task<TeamModel?> GetTeamAsync(string teamId);
        Task UpdateTeamScoreAsync(string teamId, int score);
        Task SetTeamLeaderAsync(string teamId, UserModel teamLeader);
        Task AddTeammateAsync(string teamId, UserModel teammate);
        Task RemoveTeammateAsync(string teamId, string teammateId);
        //Task DeleteTeamAsync(string teamId);
    }
}
