using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Games.Contracts;
using DrawOutApp.Server.Services;
using DrawOutApp.Server.Services.Contracts;

namespace DrawOutApp.Server.Games
{
    public class EndRoundCommand(IGameService gameService, IUserService userService, string gameId, GameTimers gameTimers) : IGameCommand
    {
        private readonly IGameService _gameService = gameService;
        private readonly IUserService _userService = userService;
        private readonly string _gameId = gameId;
        private readonly GameTimers _gameTimers = gameTimers;

        public async Task ExecuteAsync()
        {
            var gameRound = await _gameService.GetGameRoundAsync(_gameId);
            var gameModel = await _gameService.GetGameModelAsync(_gameId);

            gameRound.GameState = GameState.RoundEnded.ToString();
            var updatedModel = await _gameService.UpdateGameRoundAsync(gameRound)
                ?? throw new Exception("Error updating game round");

            await _gameService.SendGameUpdateAsync(updatedModel);

            await Task.Delay(3000);

            if(gameRound.CurrentRound < gameModel.TotalRounds)
            {
                await _userService.RemoveRolesAsync(gameRound.CurrentPainter!, [Role.Painter]);
                updatedModel.CurrentPainter = gameModel.PainterOrder![gameRound.CurrentRound];;

                await _gameService.InitNextRoundAsync(updatedModel, _gameTimers);
            }
            else
            {
                await _gameService.EndGameAsync(_gameId);
            }
        }
    }
}
