using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Games.Contracts;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Services;
using DrawOutApp.Server.Services.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace DrawOutApp.Server.Games
{
    public class InitializeNextRoundCommand(IGameService gameService, IUserService userService, IHubClients hubClients, GameTimers gameTimers, GameRoundModel gameRoundModel) : IGameCommand
    {
        private readonly IGameService _gameService = gameService;
        private readonly IUserService _userService = userService;
        private readonly IHubClients _hubClients = hubClients;
        private readonly GameTimers _gameTimers = gameTimers;
        private readonly GameRoundModel _gameRoundModel = gameRoundModel;

        public async Task ExecuteAsync()
        {
            await _userService.AddRolesAsync(_gameRoundModel.CurrentPainter!, [Role.Painter]);

            var currentPainter = _gameRoundModel.CurrentPainter!;
            var gameId = _gameRoundModel._gameId;

            _gameRoundModel.CurrentRound++;
            _gameRoundModel.GameState = GameState.Standby.ToString();

            
            var updatedModel = await _gameService.UpdateGameRoundAsync(_gameRoundModel) 
                ?? throw new Exception("Error updating game round");
            await _gameService.SendGameUpdateAsync(updatedModel);
            await _gameService.SendWordSelectPromptAsync(currentPainter, gameId);

            var isDone = await _gameTimers.StartWordSelectTimer(gameId, 18, _hubClients);

            await _gameService.StartRoundAsync(gameId, _gameTimers);
        }
    }
}
