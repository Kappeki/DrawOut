using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Games.Contracts;
using DrawOutApp.Server.Services;
using DrawOutApp.Server.Services.Contracts;

namespace DrawOutApp.Server.Games
{
    public class EndGameCommand(IGameService gameService, IUserService userService, string gameId) : IGameCommand
    {
        private readonly IGameService _gameService = gameService;
        private readonly IUserService _userService = userService;
        private readonly string _gameId = gameId;

        public async Task ExecuteAsync()
        {
            var gameRound = await _gameService.GetGameRoundAsync(_gameId);
            var gameModel = await _gameService.GetGameModelAsync(_gameId);

            gameRound.GameState = GameState.WaitingForPlayers.ToString();
            var updatedModel = await _gameService.UpdateGameRoundAsync(gameRound)
                ?? throw new Exception("Error updating game round");


            foreach (var userIds in gameModel.PainterOrder!)
            {
                await _userService.RemoveRolesAsync(userIds, [Role.Painter, Role.TeamLeader, Role.Blue, Role.Red]);
            }

            await _gameService.SendGameEndAsync(_gameId);

            await _gameService.SendGameUpdateAsync(updatedModel);
        }
    }
}
