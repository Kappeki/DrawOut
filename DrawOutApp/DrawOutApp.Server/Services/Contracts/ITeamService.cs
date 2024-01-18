using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Services.Contracts
{
    public interface ITeamService
    {
        Task <TeamModel> CreateTeamAsync();
        Task <Result<TeamModel?,string>> GetTeamAsync(string teamId);
        Task <Result<bool,string>> UpdateTeamScoreAsync(string teamId, int score);
        Task <Result<bool,string>> SetTeamLeaderAsync(string teamId, string tleaderId);
        Task <Result<bool,string>> AddTeammateAsync(string teamId, string teammateId);
        Task RemoveTeammateAsync(string teamId, string teammateId);
        
        Task AssignGameSession(TeamModel team, string gameSessionId);

        //Task DeleteTeamAsync(string teamId);
    }
}
