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
        private readonly IUserService _userService;

        public TeamService(ITeamRepo teamRepository, IUserService userService)
        {
            _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<Result<bool, string>> AddTeammateAsync(string teamId, string teammateId)
        {
            try 
            { 
                var (isErrorTeam, teamModel, errorTeam) = await GetTeamAsync(teamId);
                var (isErrorUser, teammateModel, errorUser) = await _userService.GetUserAsync(teammateId);

                if (isErrorTeam)
                    return $"Could not retrieve team with error : {errorTeam}";

                if (isErrorUser)
                    return $"Teammate not found with error {errorUser}";

                //ovo ne bi trebalo nikad da se desi, jer nestaje dugme nakon sto se klikne join na taj team
                if (teamModel!.Teammates.Contains(teammateModel!))
                    return "Teammate already in the team.";

                //isto nikad jer za taj pun team se ne vidi dugme za join
                if (teamModel.IsFull())
                   return "Team is full.";
            
                if(teammateModel!.Roles.Contains(Role.Player) && !string.IsNullOrEmpty(teammateModel.TeamId))
                {
                    await RemoveTeammateAsync(teammateModel.TeamId, teammateId);
                }

                teamModel.Teammates.Add(teammateModel);
                teammateModel.TeamId = teamId;

                var (isUpdateError, success, error) = await _userService.UpdateUserAsync(teammateId, teammateModel);
                if(isUpdateError)
                    return $"Could not update user data. : {error}";

                if (success) {
                    await _userService.AddRole(teammateId, Role.Player);
                    await _teamRepository.UpdateTeamAsync(teamId, TeamMapper.ToEntity(teamModel));
                    return true;
                }
                else
                {
                    teamModel.Teammates.Remove(teammateModel);
                    await _userService.RemoveRole(teammateId, Role.Player);
                    throw new Exception("Couldn't update user");
                }
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error adding teammate. : {error}";
            }
            return false;
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

        public async Task<Result<TeamModel?, string>> GetTeamAsync(string teamId)
        {
            TeamModel teamModel = default!;
            string errors = String.Empty;
            try
            {
                var team = await _teamRepository.GetTeamAsync(teamId);
                var (isError,teamLeader,error) = await _userService.GetUserAsync(team!.TeamLeaderId);
                if (isError)
                    return $"Could not retrieve team leader with error : {error}";
                teamModel = new TeamModel
                {
                    TeamId = team.TeamId,
                    GameSessionId = team.GameSessionId,
                    Score = team.Score,
                    TeamLeader = teamLeader!,
                    Teammates = new List<UserModel>()
                };

                foreach (var teammateId in team.TeammateIds)
                {
                    var (isErrorUser,teammate,errorUser) = await _userService.GetUserAsync(teammateId);
                    if (isErrorUser)
                        errors += $"Could not retrieve teammate with error : {errorUser}";
                    else
                        teamModel.Teammates.Add(teammate!);
                }
            }
            catch(Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error retrieving team with error : {error}\n with user errors {errors}";
            }
            return teamModel;
        }

        public async Task RemoveTeammateAsync(string teamId, string teammateId)
        {
            var teamModel = await GetTeamAsync(teamId);
            var userModel = await _userService.GetUserAsync(teammateId);
            if (teamModel.IsError)
                throw new ArgumentException("Team not found.");
            if(userModel.IsError)
                throw new ArgumentException("Teammate not found.");
            
            teamModel.Data!.Teammates.Remove(userModel.Data!);
            userModel.Data!.TeamId = String.Empty;

            await _userService.UpdateUserAsync(teammateId, userModel.Data!);
            await _userService.RemoveRole(teammateId, Role.Player);

            await _teamRepository.UpdateTeamAsync(teamId, TeamMapper.ToEntity(teamModel.Data));
        }

        public async Task<Result<bool, string>> SetTeamLeaderAsync(string teamId, string tleaderId)
        {
            try
            {
                var teamModel = await GetTeamAsync(teamId);
                var leaderModel = await _userService.GetUserAsync(tleaderId);
                if (teamModel.IsError)
                    return $"Team not found : {teamModel.Error}";
                if (leaderModel.IsError)
                    return $"User not found : {leaderModel.Error}";

                //ova dva ne bi trebalo jer dugme za set leader nestaje nakon sto se klikne na njega
                if (teamModel.Data!.TeamLeader != null)
                    return "Team already has a leader!";

                //mora da prvo igrac bude u tim da bi se postavio kao lider
                if (!teamModel.Data.Teammates.Contains(leaderModel.Data!))
                    return "User is not in the team.";


                teamModel.Data.TeamLeader = leaderModel.Data!;
                await _userService.AddRole(tleaderId, Role.TeamLeader);
                await _teamRepository.UpdateTeamAsync(teamId, TeamMapper.ToEntity(teamModel.Data));

                return true;
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error setting team leader. : {error}";
            }
            return false;
        }

        public async Task<Result<bool, string>> UpdateTeamScoreAsync(string teamId, int score)
        {
            try
            {
                var team = await _teamRepository.GetTeamAsync(teamId);
                if (team != null)
                {
                    team.Score = score;
                    await _teamRepository.UpdateTeamAsync(teamId, team);
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

        public async Task AssignGameSession(TeamModel team, string gameSessionId)
        {
            team.GameSessionId = gameSessionId;
            await _teamRepository.UpdateTeamAsync(team.TeamId, TeamMapper.ToEntity(team));
        }
    }
}
