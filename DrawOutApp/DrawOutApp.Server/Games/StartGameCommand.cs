using AutoMapper;
using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Games.Contracts;
using DrawOutApp.Server.Services;
using DrawOutApp.Server.Services.Contracts;

namespace DrawOutApp.Server.Games
{
    public class StartGameCommand(IGameService gameService, IUserService userService, string gameId, GameTimers gameTimers) : IGameCommand
    {
        private readonly IGameService _gameService = gameService;
        private readonly IUserService _userService = userService;
        private readonly string _gameId = gameId;
        private readonly GameTimers _gameTimers = gameTimers;

        public async Task ExecuteAsync()
        {
            var gameRoundModel = await _gameService.GetGameRoundAsync(_gameId);
            var gameModel = await _gameService.GetGameModelAsync(_gameId);

            if (gameRoundModel == null || gameModel == null)
            {
                throw new Exception("Game not found");
            }

            await _userService.AddRolesAsync(gameModel.TeamLeaders!["Red"], [Role.TeamLeader]);
            await _userService.AddRolesAsync(gameModel.TeamLeaders!["Blue"], [Role.TeamLeader]);

            var isComplete = await _gameService.SendGameLoadAsync(gameRoundModel, gameModel);
            if (!isComplete) throw new Exception("Error loading game! ERROR!");

            await _gameService.InitNextRoundAsync(gameRoundModel, _gameTimers);
        }
    }
}
