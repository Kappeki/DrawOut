
using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Mappers;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Services.Contracts;

namespace DrawOutApp.Server.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepo _gameRepo;
        private readonly ITeamService _teamService;
        //private readonly IRoundService _roundService;
        public GameService(IGameRepo gameRepo, ITeamService teamService)
        {
            _gameRepo = gameRepo;
            _teamService = teamService;
        }

        public async Task<Result<GameModel,string>> CreateGameAsync(string roomId)
        {
            try
            {
                var redTeam = await _teamService.CreateTeamAsync();

                var blueTeam = await _teamService.CreateTeamAsync();

                var game = new Game
                {
                    RoomId = roomId,
                    RedTeamId = redTeam.TeamId,
                    BlueTeamId = blueTeam.TeamId,
                    TotalRounds = 8,
                    CurrentRoundIndex = 0
                };

                var error = await _gameRepo.AddGameAsync(game);
                if (error)
                    return "Error occured while creating game! Transaction failed!";

                await _teamService.AssignGameSession(redTeam, game.GameSessionId);

                await _teamService.AssignGameSession(blueTeam, game.GameSessionId);

                //mozda treba da se doda nesto za runde 
                return new GameModel
                {
                    GameSessionId = game.GameSessionId,
                    RoomId = game.RoomId,
                    RedTeam = redTeam,
                    BlueTeam = blueTeam,
                    CurrentRoundIndex = game.CurrentRoundIndex
                };
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error creating game. : {error}";
            }
        }

        public async Task DeleteGameAsync(string gameSessionId)
        {
            await _gameRepo.DeleteGameAsync(gameSessionId);
        }

        public async Task<Result<GameModel?,string>> GetGameAsync(string gameSessionId)
        {
            GameModel gameModel = default!;
            try
            {
                var game = await _gameRepo.GetGameAsync(gameSessionId);
                if (game == null)
                    return "Game not found!";

                var redTeam = await _teamService.GetTeamAsync(game.RedTeamId);
                if(redTeam.IsError) return $"Error getting red team. : {redTeam.Error}";
                var blueTeam = await _teamService.GetTeamAsync(game.BlueTeamId);
                if(blueTeam.IsError) return $"Error getting blue team. : {blueTeam.Error}";
                //for now skipping the round implementation
                var rounds = new List<RoundModel>();
                gameModel =  GameMapper.ToModel(game, blueTeam.Data!, redTeam.Data!, rounds);
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error getting game. : {error}";
            }
            return gameModel;
        }

        public async Task<Result<bool, string>> UpdateGameAsync(string gameSessionId, GameModel gameModel)
        {
            try 
            { 
                var game = GameMapper.ToEntity(gameModel);
                var success = await _gameRepo.UpdateGameAsync(gameSessionId, game);
                if (success) return true;
                return false; 
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error updating game. : {error}";
            }
        }
    }
}
