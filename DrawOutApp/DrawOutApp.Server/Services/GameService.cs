
using AutoMapper;
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
        private readonly IMapper _mapper;
        public GameService(IGameRepo gameRepo, IMapper mapper)
        {
            _mapper = mapper;
            _gameRepo = gameRepo;
        }
        public async Task<string> CreateGameAsync(GameModel gameModel)
        {
            return await _gameRepo.SaveGameAsync(_mapper.Map<Game>(gameModel));
        }
        public async Task<Result<GameModel?,string>> GetGameAsync(string gameSessionId)
        {
            var game = await _gameRepo.GetGameAsync(gameSessionId);
            if (game == null) return "No game found! ERROR!";
            return _mapper.Map<GameModel>(game);
        }
        public async Task<Result<bool,string>> UpdateGameRoundAsync(GameRound gameRound)
        {
            if(gameRound == null) return "Game round is null! ERROR!";
            var success = await _gameRepo.UpdateGameRoundAsync(gameRound);
            if (success) return true;
            return "Error updating game round! ERROR!";
        }
        public async Task<Result<int, string>> IncrementScoreAsync(string gameId, string teamName, int incrementValue)
        {
            if (gameId == null) return "GameId is null! ERROR!";
            if (teamName == null) return "TeamName is null! ERROR!";
            if (incrementValue < 0) return "IncrementValue is negative! ERROR!";
            var score = await _gameRepo.IncrementScoreAsync(gameId, teamName, incrementValue);
            if (score == -1) return "Error incrementing score! ERROR!";
            return score;
        }
        public async Task<Result<bool, string>> DecrementTimerAsync(string gameId, string timerName, int decrementValue)
        {
            if (gameId == null) return "GameId is null! ERROR!";
            if (timerName == null) return "TimerName is null! ERROR!";
            if (decrementValue < 0) return "DecrementValue is negative! ERROR!";
            await _gameRepo.DecrementTimerAsync(gameId, timerName, decrementValue);
            return true;
        }
        public async Task<bool> CheckGuessAsync(string gameId, string guess)
        {
            var game = await _gameRepo.GetGameAsync(gameId);
            if (game == null) throw new Exception("Game not found! ERROR!");
            return game.SelectedWord == guess;
        }
        public async Task SetWordAsync(string gameId, string selectedWord)
        {
            var success = await _gameRepo.UpdateSelectedWordAsync(gameId, selectedWord);
            if(!success) throw new Exception("Error setting word! ERROR!");
        }
        public async Task DeleteGameAsync(string gameSessionId)
        {
            await _gameRepo.DeleteGameAsync(gameSessionId);
        }

    }
}
