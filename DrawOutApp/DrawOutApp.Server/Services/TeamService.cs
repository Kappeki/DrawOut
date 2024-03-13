using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Mappers;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Services.Contracts;

namespace DrawOutApp.Server.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepo _teamRepository;
        private readonly IUserRepo _userRepository;

        public TeamService(ITeamRepo teamRepository, IUserRepo userRepo)
        {
            _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
            _userRepository = userRepo ?? throw new ArgumentNullException(nameof(teamRepository));
        }
        public async Task<TeamModel> CreateTeamAsync(string gameSeshId, TimeSpan? expiry = null)
        {
            var team = new Team
            {
                GameSessionId = gameSeshId,
                Score = 0
            };
            await _teamRepository.AddOrUpdateTeamAsync(team, expiry);
            return TeamMapper.ToModel(team);
        }

        public async Task<Result<TeamModel?, string>> GetTeamAsync(string teamId)
        {
            TeamModel teamModel = default!;
            string errors = String.Empty;
            try
            {
                var team = await _teamRepository.GetTeamAsync(teamId);
                if(team == null)
                    return "Team not found!";
                var user = await _userRepository.GetUserAsync(team.TeamLeaderId!);
                if(user == null)
                    errors += "Team leader not assigned!\n";

                var teammates = new List<UserModel>();

                foreach (var tid in team.TeammateIds!)
                {
                    var teammate = await _userRepository.GetUserAsync(tid);
                    if(teammate == null)
                        errors += $"Teammate with id {tid} not found!\n";
                    teammates.Add(UserMapper.ToModel(teammate!));
                }

                teamModel = TeamMapper.ToModel(team, UserMapper.ToModel(user!), teammates);
            }
            catch(Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error retrieving team with error : {error}\n with user errors {errors}";
            }
            return teamModel;
        }

        public async Task<Result<bool, string>> AddTeammateAsync(string teamId, string teammateId)
        {
            try
            {
                var team = await _teamRepository.GetTeamAsync(teamId);
                if (team == null)
                    return "Team not found!";
                var user = await _userRepository.GetUserAsync(teammateId);
                if (user == null)
                    return "User not found!";


                if(team.TeammateIds!.Count == 4)
                    return "Team is full.";
               
                await _teamRepository.AddTeammateAsync(teamId, teammateId);
                return true;

            }
            catch(Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error adding teammate. : {error}";
            }

            return false;
        }
        
        public async Task<Result<bool,string>> RemoveTeammateAsync(string teamId, string teammateId)
        {
            try
            {
                var team = await _teamRepository.GetTeamAsync(teamId);
                if (team == null)
                    return "Team not found!";
                if (team.TeammateIds!.Contains(teammateId))
                {
                    await _teamRepository.RemoveTeammateAsync(teamId, teammateId);
                }
                else
                {
                    return $"User is not in the team with id : {teamId}.";
                }

                return true;
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error adding teammate. : {error}";
            }
            return false;
        }

        public async Task<Result<bool, string>> SetTeamLeaderAsync(string teamId, string tleaderId)
        {
            try
            {
                var team = await _teamRepository.GetTeamAsync(teamId);
                if (team != null)
                {
                    await _teamRepository.UpdateTeamLeaderAsync(teamId, tleaderId);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error setting team leader. : {error}";
            }
        }

        public async Task<Result<bool, string>> UpdateTeamScoreAsync(string teamId, int score)
        {
            try
            {
                var team = await _teamRepository.GetTeamAsync(teamId);
                if (team != null)
                {
                   await _teamRepository.UpdateScoreAsync(teamId, score);
                   return true;
                }
                return false;
            }
            catch(Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error updating team score. : {error}";
            }
        }
    }
}
