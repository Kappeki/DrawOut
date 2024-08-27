
using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Mappers;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Services.Contracts;
using StackExchange.Redis;

namespace DrawOutApp.Server.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepo _gameRepo;
        public GameService(IGameRepo gameRepo)
        {
            _gameRepo = gameRepo;
        }

        public async Task<Result<GameModel,string>> CreateGameAsync(string roomId)
        {
            throw new NotImplementedException();
            /*var tran = _gameRepo.BeginTransaction();
            try
            {
                var game = new Game()
                {
                    RoomId = roomId,
                    TotalRounds = 0,
                    CurrentRoundIndex = 0
                };
                var blueTeam = new Team()
                {
                    GameSessionId = game._cacheKey,
                    Score = 0
                };
                var redTeam = new Team()
                {
                    GameSessionId = game._cacheKey,
                    Score = 0
                };
                game.BlueTeamId = blueTeam._cacheKey;
                game.RedTeamId = redTeam._cacheKey;

                _teamRepo.AddOrUpdateTeamAsync(blueTeam, tran);
                _teamRepo.AddOrUpdateTeamAsync(redTeam, tran);
                _gameRepo.AddGameAsync(game, tran);

                bool success = await tran.ExecuteAsync();
                if (!success) return "Error creating game.";

                return GameMapper.ToModel(game, 
                    TeamMapper.ToModel(blueTeam),
                    TeamMapper.ToModel(redTeam));
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error creating game. : {error}";
            }*/
        }

        public async Task DeleteGameAsync(string gameSessionId)
        {
            await _gameRepo.DeleteGameAsync(gameSessionId);
        }

        public async Task<Result<GameModel?,string>> GetGameAsync(string gameSessionId)
        {
            throw new NotImplementedException();
            
            /*GameModel gameModel = default!;
            try
            {
                var game = await _gameRepo.GetGameAsync(gameSessionId);
                if (game == null)
                    return "Game not found!";

                var blueTeam = await _teamRepo.GetTeamAsync(game.BlueTeamId!);
                var redTeam = await _teamRepo.GetTeamAsync(game.RedTeamId!);

                gameModel = GameMapper.ToModel(game, 
                            TeamMapper.ToModel(blueTeam!),
                            TeamMapper.ToModel(redTeam!));

            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error getting game. : {error}";
            }
            return gameModel;*/
        }

        /*public async Task<Result<bool, string>> UpdateGameAsync(string gameSessionId, GameModel gameModel)
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
        }*/
    }
}
