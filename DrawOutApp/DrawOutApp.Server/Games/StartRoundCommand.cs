using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Games.Contracts;
using DrawOutApp.Server.Services;
using DrawOutApp.Server.Services.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace DrawOutApp.Server.Games
{
    public class StartRoundCommand(IGameService gameService, IHubClients hubClients, string gameId, GameTimers gameTimers) : IGameCommand
    {
        private readonly IGameService _gameService = gameService;
        private readonly IHubClients _hubClients = hubClients;
        private readonly string _gameId = gameId;
        private readonly GameTimers _gameTimers = gameTimers;

        public async Task ExecuteAsync()
        {
            var gameRound = await _gameService.GetGameRoundAsync(_gameId);
            var gameModel = await _gameService.GetGameModelAsync(_gameId);
            gameRound.GameState = GameState.InProgress.ToString();

            var currentPainter = gameRound.CurrentPainter;
            var updatedRound = await _gameService.UpdateGameRoundAsync(gameRound) 
                ?? throw new Exception("Error updating game round! ERROR!");
            await _gameService.SendPermitToPlayersAsync(currentPainter!, _gameId);
            await _gameService.SendGameUpdateAsync(updatedRound);


            var isDone = await _gameTimers.StartMainTimer(_gameId, gameModel.MainTimer, _hubClients);
            if (isDone)
            {
                await _gameService.EndRoundAsync(_gameId, _gameTimers);
            }
            else
            {
                await _gameService.StartStealAsync(_gameId, _gameTimers);
            }
        }
    }
}
