
using Amazon.Runtime.Internal.Transform;
using AutoMapper;
using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Hubs;
using DrawOutApp.Server.Mappers;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Repositories;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Services.Contracts;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using StackExchange.Redis;
using System.Linq;
using Role = DrawOutApp.Server.Entities.Role;

namespace DrawOutApp.Server.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepo _gameRepo;
        private readonly IMapper _mapper;

        private readonly IUserService _userService;
        private readonly GameFlowService _gameFlowService;
        private readonly IHubContext<GameHub> _hubContext;

        public GameService(
            IGameRepo gameRepo, 
            IMapper mapper, 
            IUserService userService, 
            IHubContext<GameHub> hubContext,
            GameFlowService gameFlowService
            )
        {
            _mapper = mapper;
            _gameRepo = gameRepo;
            _userService = userService;
            _hubContext = hubContext;
            _gameFlowService = gameFlowService;
        }


        //game flow mechanics
        public async Task StartGameAsync(string gameId, GameTimers gameTimers)
        {
            _gameFlowService.EnqueueEvent(async () =>
            {
                var game = await _gameRepo.GetGameAsync(gameId);
                if (game == null) throw new Exception("Game not found! ERROR!");
                var gameRoundModel = _mapper.Map<GameRoundModel>(game);
                var gameModel = _mapper.Map<GameModel>(game);
            
                await _userService.AddRolesAsync(gameModel.TeamLeaders!["Red"], [Role.TeamLeader]);
                await _userService.AddRolesAsync(gameModel.TeamLeaders!["Blue"], [Role.TeamLeader]);

                var isComplete = await SendGameLoadAsync(gameRoundModel, gameModel);
                if (!isComplete) throw new Exception("Error loading game! ERROR!");

    
                await InitNextRound(gameRoundModel, gameTimers);
            });
        }
        private async Task InitNextRound(GameRoundModel gameRoundModel, GameTimers gameTimers)
        {
            _gameFlowService.EnqueueEvent(async () =>
            {
                await _userService.AddRolesAsync(gameRoundModel.CurrentPainter!, [Role.Painter]);

                var currentPainter = gameRoundModel.CurrentPainter!;
                var gameId = gameRoundModel._gameId;

                gameRoundModel.CurrentRound++;
                gameRoundModel.GameState = GameState.Standby.ToString();
            
                var updatedModel = await _gameRepo.UpdateGameRoundAsync(gameRoundModel);
                if (updatedModel == null) throw new Exception("Error updating game round! ERROR!");

                await SendGameUpdateAsync(_mapper.Map<GameRoundModel>(updatedModel));
                await SendWordSelectPromptAsync(currentPainter, gameId);
                //ceka se kraj tajmera za svaki sluc
                var isDone = await gameTimers.StartWordSelectTimer(gameId, 18, _hubContext.Clients);

                await StartRound(gameId, gameTimers);
            });
        }
        private async Task StartRound(string gameId, GameTimers gameTimers)
        {
            _gameFlowService.EnqueueEvent(async () =>
            {
                var game = await _gameRepo.GetGameAsync(gameId);
                var gameRound = _mapper.Map<GameRoundModel>(game);

                gameRound.GameState = GameState.InProgress.ToString();
                var currentPainter = gameRound.CurrentPainter;
                var updatedModel = await _gameRepo.UpdateGameRoundAsync(gameRound);
                if (updatedModel == null) throw new Exception("Error updating game round! ERROR!");

                await SendPermitToPlayersAsync(currentPainter!, gameId);
                await SendGameUpdateAsync(_mapper.Map<GameRoundModel>(updatedModel));

                var isDone = await gameTimers.StartMainTimer(gameId, game!.MainTimer, _hubContext.Clients);
                if(isDone)
                {
                    await EndRound(gameId, gameTimers);
                }
                else
                {
                    await StartSteal(gameId, gameTimers);
                }
            });
        }
        private async Task StartSteal(string gameId, GameTimers gameTimers)
        {
            _gameFlowService.EnqueueEvent(async () =>
            {
                var game = await _gameRepo.GetGameAsync(gameId);
                var gameRound = _mapper.Map<GameRoundModel>(game);

                gameRound.GameState = GameState.Steal.ToString();
                var currentPainter = gameRound.CurrentPainter;
            
                var updatedModel = await _gameRepo.UpdateGameRoundAsync(gameRound);
                if (updatedModel == null) throw new Exception("Error updating game round! ERROR!");

                await SendGameUpdateAsync(_mapper.Map<GameRoundModel>(updatedModel));
                await SendStealPermitAsync(currentPainter!, gameId);

                var isDone = await gameTimers.StartStealTimer(gameId, game!.StealTimer, _hubContext.Clients);

                await EndRound(gameId,gameTimers);
            });
        }
        private async Task EndRound(string gameId, GameTimers gameTimers)
        {
            _gameFlowService.EnqueueEvent(async () =>
            {
                var game = await _gameRepo.GetGameAsync(gameId);
                var gameRound = _mapper.Map<GameRoundModel>(game);

                gameRound.GameState = GameState.RoundEnded.ToString();
                var updatedModel = await _gameRepo.UpdateGameRoundAsync(gameRound);
                if (updatedModel == null) throw new Exception("Error updating game round! ERROR!");

                await SendGameUpdateAsync(_mapper.Map<GameRoundModel>(updatedModel));

                if (gameRound.CurrentRound < game!.TotalRounds)
                {
                    await _userService.RemoveRolesAsync(gameRound.CurrentPainter!, [Role.Painter]);

                    var gameRoundModel = _mapper.Map<GameRoundModel>(game);
                    gameRoundModel.CurrentPainter = game.PainterOrder![gameRoundModel.CurrentRound];

                    _ = Task.Delay(3000);

                    await InitNextRound(gameRoundModel, gameTimers);
                }
                else
                {
                    await EndGame(gameId);
                }
            });
        }
        private async Task EndGame(string gameId)
        {
            _gameFlowService.EnqueueEvent(async () =>
            {
                var game = await _gameRepo.GetGameAsync(gameId);
                if (game == null) throw new Exception("Game not found! ERROR!");
                var gameRound = _mapper.Map<GameRoundModel>(game);
                gameRound.GameState = GameState.WaitingForPlayers.ToString();
                var updatedModel = await _gameRepo.UpdateGameRoundAsync(gameRound);
                if (updatedModel == null) throw new Exception("Error updating game round! ERROR!");

                await SendGameUpdateAsync(_mapper.Map<GameRoundModel>(updatedModel));
                
                foreach (var userIds in game.PainterOrder!)
                {
                    await _userService.RemoveRolesAsync(userIds, [Role.Painter, Role.TeamLeader, Role.Blue, Role.Red]);
                }

                await _hubContext.Clients.Group(gameId).SendAsync("GameEnded", "Waiting");
            });

            //ovo bi trebalo cim se ubaci u GameHistory, za to mora provera da li soba ima password 
            //game.CurrentPainter = null;
            //game.SelectedWord = null;
            //game.CurrentRound = 0;
            //game.PainterOrder = null;
            //game.TeamLeaders = null;
            //game.TotalRounds = 0;
        }

        //public methods are revealed to the clients through the hub
        public async Task SelectWordAsync(string gameId, string word, GameTimers gameTimers)
        {
            var game = await _gameRepo.GetGameAsync(gameId);
            var gameRound = _mapper.Map<GameRoundModel>(game);
            if (game!.GameState != GameState.Standby)
                return; // Ignore if not in word selection phase

            gameRound.SelectedWord = word;
            var updatedModel = await _gameRepo.UpdateGameRoundAsync(gameRound);
            if (updatedModel == null) throw new Exception("Error updating game round! ERROR!");

            await SendGameUpdateAsync(_mapper.Map<GameRoundModel>(updatedModel));

            await _hubContext.Clients.Group(gameId).SendAsync("WordSelected", updatedModel.SelectedWord!.Length);

            await gameTimers.StopWordSelectTimer(gameId, _hubContext.Clients);
        }
        public async Task SubmitGuessAsync(string userId, string gameId, string guess, GameTimers gameTimers)
        {
            var isCorrect = await CheckGuessAsync(gameId, guess);
            if (isCorrect)
            {
                var game = await _gameRepo.GetGameAsync(gameId);
                if (game == null) throw new Exception("Game not found! ERROR!");

                var (pError, painter, perror) = await _userService.GetUserAsync(game.CurrentPainter!);
                if (pError) throw new Exception("Painter not found! ERROR!" + perror);

                var painterTeam = painter!.Roles!.Contains("Red") ? "Red" : "Blue";

                var (isError, user, error) = await _userService.GetUserAsync(userId);
                if (isError) throw new Exception("Painter not found! ERROR!" + error);

                var winningTeam = user!.Roles!.Contains("Red") ? "Red" : "Blue";
                
                if(painterTeam != winningTeam)
                {
                    await IncrementScoreAsync(gameId, winningTeam, 75);
                }
                else
                {
                    await IncrementScoreAsync(gameId, winningTeam, 100);
                }

                await _hubContext.Clients.Group(gameId).SendAsync("CorrectGuess", winningTeam, guess);

                await gameTimers.StopRunningTimer(gameId, _hubContext.Clients);
            }
        }

        //hub notifications
        private async Task<bool> SendGameLoadAsync(GameRoundModel gameRoundModel, GameModel gameModel)
        {
            try
            {
                await _hubContext.Clients.Group(gameModel._id).SendAsync("LoadGame", gameModel, gameRoundModel);
                return true;
            }
            catch (Exception ex)
            {
                throw new HubException("Error loading game! ERROR!" + ex.Message);
            }
        }
        private async Task SendWordSelectPromptAsync(string painterId, string gameId)
        {

            try
            {
                var painterConnId = await _userService.GetConnectionIdAsync(painterId);
                if(painterConnId == null) throw new Exception("Painter connectionId not found! ERROR!");
                //disable po default na pocetak runde da ne moze da se pogadja
                await _hubContext.Clients.Group(gameId).SendAsync("EnableGuessing", false, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                await _hubContext.Clients.Client(painterConnId).SendAsync("PromptWordSelect", true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                await _hubContext.Clients.AllExcept(painterConnId).SendAsync("PromptWordSelect", false, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                
            }
            catch (Exception ex)
            {
                throw new HubException("Error starting word selection! ERROR!" + ex.Message);
            }
        }
        private async Task SendGameUpdateAsync(GameRoundModel gameRoundModel)
        {
            try
            {
                await _hubContext.Clients.Group(gameRoundModel._gameId).SendAsync("UpdateGame", gameRoundModel);
            }
            catch (Exception ex)
            {
                throw new HubException("Error updating game! ERROR!" + ex.Message);
            }
        }
        private async Task SendPermitToPlayersAsync(string painterId, string gameId)
        {
            try
            {
                var guessingTeamIds = await GetGuessingTeamAsync(gameId);
                var guessingTeamConnIds = new List<string>();
                foreach (var userId in guessingTeamIds)
                {
                    var connId = await _userService.GetConnectionIdAsync(userId);
                    if (connId == null) throw new Exception("ConnectionId not found! ERROR!");
                    guessingTeamConnIds.Add(connId);
                }
                var painterConnId = await _userService.GetConnectionIdAsync(painterId);
                if (painterConnId == null) throw new Exception("Painter connectionId not found! ERROR!");
                await _hubContext.Clients.Clients(guessingTeamConnIds).SendAsync("EnableGuessing", true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                await _hubContext.Clients.GroupExcept(gameId, painterConnId).SendAsync("EnableDrawing", false, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                await _hubContext.Clients.Client(painterConnId).SendAsync("EnableDrawing",true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            }
            catch (Exception ex)
            {
                throw new HubException("Error updating game! ERROR!" + ex.Message);
            }
        }
        private async Task SendStealPermitAsync(string painterId, string gameId)
        {
            try
            {
                var game = await _gameRepo.GetGameAsync(gameId);
                if (game == null) throw new Exception("Game not found! ERROR!");
                var (isError, painter, error) = await _userService.GetUserAsync(painterId);
                if (isError) throw new Exception("Painter not found! ERROR!" + error);
                
                var painterTeam = painter!.Roles!.Contains("Red") ? "Red" : "Blue";

                var opposingLeader = game.TeamLeaders![painterTeam == "Red" ? "Blue" : "Red"];

                var opposingLeaderConnId = await _userService.GetConnectionIdAsync(opposingLeader);
                if (opposingLeaderConnId == null) throw new Exception("Opposing leader connectionId not found! ERROR!");

                var painterConnId = await _userService.GetConnectionIdAsync(painterId);
                if (painterConnId == null) throw new Exception("Painter connectionId not found! ERROR!");

                await _hubContext.Clients.Client(painterConnId).SendAsync("EnableDrawing", false, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                await _hubContext.Clients.GroupExcept(gameId, opposingLeaderConnId).SendAsync("EnableGuessing", false, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                await _hubContext.Clients.Client(opposingLeaderConnId).SendAsync("EnableGuessing", true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            }
            catch (Exception ex)
            {
                throw new HubException("Error updating game! ERROR!" + ex.Message); ;
            }
        }
        //------------------
       
        //default business methods
        public async Task<GameRoundModel> GetGameRoundAsync(string gameId)
        {
            var game = await _gameRepo.GetGameAsync(gameId);
            if (game == null) throw new Exception("Game not found! ERROR!");
            return _mapper.Map<GameRoundModel>(game);
        }
        public async Task<GameRoundModel> CreateGameAsync(GameModel gameModel, List<string> userIds)
        {
            List<UserModel> users = new();
            foreach (var userId in userIds)
            {
                var (isError, user, error) = await _userService.GetUserAsync(userId);
                if (isError) throw new Exception("User not found! ERROR!" + error);
                users.Add(user!);
            }
            gameModel.PainterOrder = InitializePlayerOrder(users);
            gameModel.TeamLeaders = SelectTeamLeaders(users);

            var gameRoundModel = new GameRoundModel
            {
                _gameId = gameModel._id,
                GameState = GameState.WaitingForPlayers.ToString(),
                CurrentRound = 0,
                CurrentPainter = gameModel.PainterOrder[0],
                BlueScore = 0,
                RedScore = 0,
                SelectedWord = string.Empty
            };

            await _gameRepo.SaveGameAsync(_mapper.Map<Game>(gameModel), TimeSpan.FromMinutes(45));
            await _gameRepo.UpdateGameRoundAsync(gameRoundModel);

            return gameRoundModel;
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
        public async Task<bool> CheckGuessAsync(string gameId, string guess)
        {
            var game = await _gameRepo.GetGameAsync(gameId);
            if (game == null) throw new Exception("Game not found! ERROR!");
            return game.SelectedWord == guess;
        }
        
        //helper methods for object creation
        private List<string> InitializePlayerOrder(List<UserModel> users)
        {
            var redTeam = users.Where(user => user.Roles != null && user.Roles.Contains("Red")).ToList();
            var blueTeam = users.Where(user => user.Roles != null && user.Roles.Contains("Blue")).ToList();

            var combinedOrder = new List<string>();
            var random = new Random();
            var firstTeam = random.NextDouble() < 0.5 ? redTeam : blueTeam;
            var secondTeam = firstTeam == redTeam ? blueTeam : redTeam;

            int i = 0, j = 0;

            while (i < firstTeam.Count || j < secondTeam.Count)
            {
                if (i < firstTeam.Count)
                {
                    combinedOrder.Add(firstTeam[i]._sessionKey);
                    i++;
                }
                if (j < secondTeam.Count)
                {
                    combinedOrder.Add(secondTeam[j]._sessionKey);
                    j++;
                }
            }

            return combinedOrder;
        }
        private Dictionary<string, string> SelectTeamLeaders(List<UserModel> users)
        {
            var redTeam = users.Where(user => user.Roles != null && user.Roles.Contains("Red")).ToList();
            var blueTeam = users.Where(user => user.Roles != null && user.Roles.Contains("Blue")).ToList();

            var random = new Random();
            var redLeader = redTeam[random.Next(redTeam.Count)];
            var blueLeader = blueTeam[random.Next(blueTeam.Count)];

            return new Dictionary<string, string>
            {
                { "Red", redLeader._sessionKey },
                { "Blue", blueLeader._sessionKey }
            };
        }
        private async Task<List<string>> GetGuessingTeamAsync(string gameId)
        {
            var game = await _gameRepo.GetGameAsync(gameId);
            if (game == null) throw new Exception("Game not found! ERROR!");

            var painterOrder = game.PainterOrder;
            var currentPainter = game.CurrentPainter;

            var userIdsNotPainting = painterOrder!.Where(userId => userId != currentPainter).ToList();

            var (isError, painter, error) = await _userService.GetUserAsync(currentPainter!);
            if (isError) throw new Exception("Painter not found! ERROR!" + error);
            var painterTeam = painter!.Roles!.Contains("Red") ? "Red" : "Blue";

            List<UserModel> guessingTeam = new();
            foreach (var userId in userIdsNotPainting)
            {
                var (uError, user, uerror) = await _userService.GetUserAsync(userId);
                if (uError) throw new Exception("User not found! ERROR!" + uerror);
                if (user!.Roles!.Contains(painterTeam)) guessingTeam.Add(user!);
            }

            return guessingTeam.Select(user => user._sessionKey).ToList();
        }
    }
}
