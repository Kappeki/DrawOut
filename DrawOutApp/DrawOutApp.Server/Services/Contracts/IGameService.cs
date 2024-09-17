using DrawOutApp.Server.Hubs;
using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Services.Contracts
{
    public interface IGameService
    {
        Task<GameRoundModel> CreateGameAsync(GameModel gameModel, List<string> userIds);
        Task<Result<int, string>> IncrementScoreAsync(string gameId, string teamName, int incrementValue);
        Task<bool> CheckGuessAsync(string gameId, string guess);
        Task<GameRoundModel> GetGameRoundAsync(string gameId);
        Task<GameModel> GetGameModelAsync(string gameId);
        Task<GameRoundModel> UpdateGameRoundAsync(GameRoundModel gameRoundModel);
        Task SelectWordAsync(string gameId, string word, GameTimers gameTimers);
        Task SubmitGuessAsync(string userId, string gameId, string guess, GameTimers gameTimers);

        //game flow logic
        Task StartGameAsync(string gameId, GameTimers gameTimers);
        Task InitNextRoundAsync(GameRoundModel gameRoundModel, GameTimers gameTimers);
        Task StartRoundAsync(string gameId, GameTimers gameTimers);
        Task StartStealAsync(string gameId, GameTimers gameTimers);
        Task EndRoundAsync(string gameId, GameTimers gameTimers);
        Task EndGameAsync(string gameId);


        // SignalR methods to notify clients
        Task<bool> SendGameLoadAsync(GameRoundModel gameRoundModel, GameModel gameModel);
        Task SendGameUpdateAsync(GameRoundModel gameRoundModel);
        Task SendWordSelectPromptAsync(string painterId, string gameId);
        Task SendPermitToPlayersAsync(string painterId, string gameId);
        Task SendStealPermitAsync(string painterId, string gameId);
        Task SendGameEndAsync(string gameId);
    }
}
