using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Services.Contracts
{
    public interface IGameService
    {
        Task<string> CreateGameAsync(GameModel gameModel);
        Task<Result<GameModel?,string>> GetGameAsync(string gameSessionId);
        Task<Result<bool, string>> UpdateGameRoundAsync(GameRound gameRound);
        Task<Result<int, string>> IncrementScoreAsync(string gameId, string teamName, int incrementValue);
        Task<Result<bool, string>> DecrementTimerAsync(string gameId, string timerName, int decrementValue);
        Task<bool> CheckGuessAsync(string gameId, string guess);
        Task SetWordAsync(string gameId, string selectedWord);
        Task DeleteGameAsync(string gameSessionId);
    }
}
