using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Services.Contracts;

namespace DrawOutApp.Server.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepo _teamRepository;
        private readonly IUserService _userService;

        public TeamService(ITeamRepo teamRepository, IUserService userService)
        {
            _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }


        public async Task AddTeammateAsync(string teamId, UserModel teammate)
        {
            var team = await _teamRepository.GetTeamAsync(teamId);
            if (team != null && teammate != null)
            {
                team.TeammateIds.Add(teammate.SessionId);
                await _teamRepository.UpdateTeamAsync(teamId, team);
            }
        }

        public async Task<TeamModel> CreateTeamAsync()
        {
            var team = new Team
            {
                TeammateIds = new List<string>(),
                GameSessionId = String.Empty,
                Score = 0
            };
            await _teamRepository.AddTeamAsync(team);
            return new TeamModel
            {
                TeamId = team.TeamId,
                GameSessionId = team.GameSessionId,
                Teammates = new List<UserModel>(),
                Score = 0
            };
        }

        public async Task<TeamModel?> GetTeamAsync(string teamId)
        {
            var team = await _teamRepository.GetTeamAsync(teamId);
            if (team == null) return null;

            var teamModel = new TeamModel
            {
                TeamId = team.TeamId,
                GameSessionId = team.GameSessionId,
                Score = team.Score,
                TeamLeader = await _userService.GetUserAsync(team.TeamLeaderId),
                Teammates = new List<UserModel>()
            };

            foreach (var teammateId in team.TeammateIds)
            {
                var teammate = await _userService.GetUserAsync(teammateId);
                if (teammate != null)
                {
                    teamModel.Teammates.Add(teammate);
                }
            }

            return teamModel;
        }

        public async Task RemoveTeammateAsync(string teamId, string teammateId)
        {
            var team = await _teamRepository.GetTeamAsync(teamId);
            if (team != null)
            {
                team.TeammateIds.Remove(teammateId);
                await _userService.RemoveRole(teammateId, Role.Player);
                await _teamRepository.UpdateTeamAsync(teamId, team);
            }
        }

        public async Task SetTeamLeaderAsync(string teamId, UserModel teamLeader)
        {
            var team = await _teamRepository.GetTeamAsync(teamId);
            if (team != null && teamLeader != null)
            {
                team.TeamLeaderId = teamLeader.SessionId;
                await _userService.AddRole(teamLeader.SessionId, Role.TeamLeader);
                await _teamRepository.UpdateTeamAsync(teamId, team);
            }
        }

        public async Task UpdateTeamScoreAsync(string teamId, int score)
        {
            var team = await _teamRepository.GetTeamAsync(teamId);
            if (team != null)
            {
                team.Score = score;
                await _teamRepository.UpdateTeamAsync(teamId, team);
            }
        }
    }
}
