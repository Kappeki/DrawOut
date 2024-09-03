using AutoMapper;
using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Services.Contracts;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DrawOutApp.Server.Hubs
{
    public class GameHub : Hub
    {
        private readonly IGameService _gameService;
        private readonly IUserService _userService;
        private readonly IRoomService _roomService;
        private readonly IMapper _mapper;
        private static readonly ConcurrentDictionary<string,
         (TaskCompletionSource<bool> MainTimerTcs,
         TaskCompletionSource<bool> StealTimerTcs,
         TaskCompletionSource<bool> WordSelectionTcs)> _gameTimers = new();
        public GameHub(IGameService gameService, IUserService userService, IMapper mapper, IRoomService roomService)
        {
            _gameService = gameService;
            _userService = userService;
            _mapper = mapper;
            _roomService = roomService;
        }

        //moglo je i ovako: 
        //cuvaju se connection ids po timovima, kako se connectuje tako mu se doda u tu listu
        //onda bi se sve slalo preko Clients.Clients sto je vrv mnogo bolje
        public async Task ConnectToGame(string gameId)
        {
            Context.Items["GameKey"] = gameId;
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
            await Clients.Group(gameId).SendAsync("UserConnected", Context.Items["Nickname"]!.ToString());
        }
        public async Task StartGame(Game game)
        {
            var gameModel = _mapper.Map<GameModel>(game);
            var gameId = await _gameService.CreateGameAsync(gameModel);
            gameModel._id = gameId;
            var gameRound = new GameRound
            {
                _gameId = gameId,
                GameState = game.GameState,
                BlueScore = 0,
                RedScore = 0,
                CurrentPainter = game.PainterOrder![0],
                CurrentRound = 0,
                SelectedWord = "",
                MainTimer = game.MainTimer,
                StealTimer = game.StealTimer
            };


            var blueLeader = game.TeamLeaders!["Blue"];
            var redLeader = game.TeamLeaders!["Red"];
            await _userService.AddRolesAsync(blueLeader, [Role.TeamLeader]);
            await _userService.AddRolesAsync(redLeader, [Role.TeamLeader]);

            _gameTimers[gameId] = (new TaskCompletionSource<bool>(), new TaskCompletionSource<bool>(), new TaskCompletionSource<bool>());

            await Clients.Group(gameId).SendAsync("LoadingGame", gameModel, gameRound);
            
            await Clients.Group(gameId).SendAsync("StartingNextRound",true,DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }
        public async Task StartNextRound(GameRound gameRound)
        {
            var gameKey = Context.Items["GameKey"]!.ToString();
            
            if (IsAdmin())
            {
                var (isError, game, error) = await _gameService.GetGameAsync(gameKey!);
                if (isError) throw new HubException("Game not found : " + error);
                var currentPainter = game!.PainterOrder![gameRound.CurrentRound];

                var (uError, painter, uerror) = await _userService.GetUserAsync(currentPainter);
                if (uError) throw new HubException("Painter not found : " + uerror);

                await _userService.AddRolesAsync(currentPainter, [Role.Painter]);

                gameRound.CurrentRound++;
                gameRound.CurrentPainter = currentPainter;
                var (gError, success, gerror) = await _gameService.UpdateGameRoundAsync(gameRound);
                if (!success) throw new HubException("Error updating game round!");

                //listener added

                await Clients.Group(gameKey!).SendAsync("RoundStandby", gameRound);
                //ovde se zakuca

                await Clients.Group(gameKey!).SendAsync("StartingSelect", true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            }
            
            //updateuju se korisnici ovako u game
            //await SendUsersInGame(gameKey);
            //--------------
            //await StartWordSelection(gameRound);
        }
        public async Task StartWordSelection(GameRound gameRound)
        {
            var gameKey = Context.Items["GameKey"]!.ToString();

            if (_gameTimers.TryGetValue(gameKey!, out var timers))
            {
                var wordSelectionTcs = new TaskCompletionSource<bool>();

                if (Context.Items["SeshKey"]!.ToString() == gameRound.CurrentPainter)
                {
                    var userRoles = Context.Items["Roles"] as List<string>;
                    userRoles?.Add("Painter");
                    Context.Items["Roles"] = userRoles;

                    await Clients.Client(Context.ConnectionId).SendAsync("PromptWordSelection", true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    await Clients.GroupExcept(gameKey!, Context.ConnectionId).SendAsync("PainterSelectingWord", true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                }

                if (IsAdmin())
                {
                    timers.WordSelectionTcs = wordSelectionTcs; 

                    var wordTimer = new Timer(async state =>
                    {
                        if (!timers.WordSelectionTcs.Task.IsCompleted)
                        {
                            timers.WordSelectionTcs.TrySetResult(false);
                        }
                    }, null, 15000, Timeout.Infinite);

                    var wordSelected = await wordSelectionTcs.Task;
                    wordTimer.Dispose();

                    if (wordSelected)
                    {
                        await Clients.Group(gameKey!).SendAsync("StartingRound", true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    }
                    else
                    {
                        await Clients.Group(gameKey!).SendAsync("HandlingNoWord", true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    }
                }
                else
                {
                    await wordSelectionTcs.Task;
                }
            }
        }
        public async Task SelectWord(string gameId, string selectedWord)
        {
            if (_gameTimers.TryGetValue(gameId, out var timers))
            {
                await _gameService.SetWordAsync(gameId, selectedWord);

                await Clients.Group(gameId).SendAsync("WordSelected", selectedWord.Length);

                if (!timers.WordSelectionTcs.Task.IsCompleted)
                {
                    timers.WordSelectionTcs.TrySetResult(true);
                }
            }
        }
        //ADMIN ONLY METHOD
        public async Task HandleNoWordSelected()
        {
            var gameKey = Context.Items["GameKey"]!.ToString();

            // Notify clients that no word was selected
            await Clients.Group(gameKey!).SendAsync("NoWordSelected", "breadsbread".Length);

            await _gameService.SetWordAsync(gameKey!, "breadsbread");

            // Ensure the word selection timer is cleaned up properly
            if (_gameTimers.TryGetValue(gameKey!, out var timers))
            {
                if (!timers.WordSelectionTcs.Task.IsCompleted)
                {
                    timers.WordSelectionTcs.TrySetResult(false);
                }
            }

            await Clients.Group(gameKey!).SendAsync("StartingRound", true);
        }
        public async Task StartRound(GameRound gameRound)
        {
            var roundTcs = new TaskCompletionSource<bool>();
            
            var gameKey = Context.Items["GameKey"]!.ToString();
            
            if (IsAdmin())
            {
                gameRound.GameState = GameState.InProgress;
                await Clients.Group(gameKey!).SendAsync("RoundStarted", gameRound);
                await _gameService.UpdateGameRoundAsync(gameRound);
            }

            var (isError, painter, error) = await _userService.GetUserAsync(gameRound.CurrentPainter!);
            var roles = Context.Items["Roles"] as List<string>;
            var painterTeam = painter!.Roles!.FirstOrDefault(r => r == Role.Blue.ToString() || r == Role.Red.ToString())!;

            if (Context.Items["SeshKey"]!.ToString() == gameRound.CurrentPainter)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("EnableDrawing", true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                await Clients.Client(Context.ConnectionId).SendAsync("EnableGuessing", false, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                await Clients.GroupExcept(gameKey!,Context.ConnectionId).SendAsync("EnableDrawing", false, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            }
            else
            {
                if(roles != null && roles.Contains(painterTeam))
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("EnableGuessing", true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                }
            }

            if (IsAdmin())  // Only the admin handles the timer
            {
                _gameTimers[gameKey!] = (MainTimerTcs: roundTcs, 
                    StealTimerTcs: _gameTimers[gameKey!].StealTimerTcs, 
                    WordSelectionTcs: _gameTimers[gameKey!].WordSelectionTcs);

                int remainingTime = gameRound.MainTimer;

                var timer = new Timer(async state =>
                {
                    remainingTime--;

                    await Clients.Group(gameKey!).SendAsync("UpdateMainTimer", remainingTime);

                    if (_gameTimers.TryGetValue(gameKey!, out var timers))
                    {
                        if (!timers.MainTimerTcs.Task.IsCompleted)
                        {
                            if (remainingTime <= 0)
                            {
                                timers.MainTimerTcs.TrySetResult(false);
                            }
                        }
                    }
                }, null, 1000, 1000); // 1-second intervals

                var correctGuess = await roundTcs.Task;
                timer.Dispose();

                if (correctGuess)
                {
                    await Clients.Group(gameKey!).SendAsync("EndingRound",true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                }
                else
                {
                    await Clients.Group(gameKey!).SendAsync("StartingSteal", true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                }
            }
            else
            {
                await roundTcs.Task;
            }
        }
        public async Task StartStealPhase(GameRound gameRound)
        {
            var stealTcs = new TaskCompletionSource<bool>();
            
            var gameKey = Context.Items["GameKey"]!.ToString();

            if (IsAdmin())
            {
                gameRound.GameState = GameState.Steal;
                await _gameService.UpdateGameRoundAsync(gameRound);
                await Clients.Group(gameKey!).SendAsync("StealPhaseStarted", gameRound);
            }

            var (isError, painter, error) = await _userService.GetUserAsync(gameRound.CurrentPainter!);
            if (isError) throw new HubException("Painter not found : " + error);

            var painterTeam = painter!.Roles!.FirstOrDefault(r => r == Role.Blue.ToString() || r == Role.Red.ToString())!;

            var roles = Context.Items["Roles"] as List<string>;

            if (Context.Items["SeshKey"]!.ToString() == gameRound.CurrentPainter)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("EnableDrawing", false, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            }

            if (roles != null && roles.Contains(Role.TeamLeader.ToString()) && !roles.Contains(painterTeam))
            {
                //FRONTEND : klijent slusa za ove dve metode i namesta na true ili false allowGuess recimo
                await Clients.GroupExcept(gameKey!, Context.ConnectionId).SendAsync("EnableGuessing", false, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                await Clients.Client(Context.ConnectionId).SendAsync("EnableGuessing", true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            }


            if (IsAdmin())
            {
                _gameTimers[gameKey!] = (MainTimerTcs: _gameTimers[gameKey!].MainTimerTcs, 
                    StealTimerTcs: stealTcs, 
                    WordSelectionTcs: _gameTimers[gameKey!].WordSelectionTcs);

                int remainingTime = gameRound.StealTimer;

                var timer = new Timer(async state =>
                {
                    remainingTime--;
                    
                    await Clients.Group(gameKey!).SendAsync("UpdateStealTimer", remainingTime);

                    if (_gameTimers.TryGetValue(gameKey!, out var timers))
                    {
                        if (!timers.StealTimerTcs.Task.IsCompleted)
                        {
                            if(remainingTime <= 0)
                            { 
                                timers.StealTimerTcs.TrySetResult(false);
                            }
                        }
                    }
                }, null, 1000, 1000); // 1-second intervals

                await stealTcs.Task;
                timer.Dispose();

                await Clients.Group(gameKey!).SendAsync("EndingRound", true);
            }
            else
            {
                await stealTcs.Task;
            }
        }
        public async Task SubmitGuess(string gameId, string guess)
        {
            var isCorrect = await _gameService.CheckGuessAsync(gameId, guess);
            if (isCorrect)
            {
                if (_gameTimers.TryGetValue(gameId, out var timers))
                {
                    if (!timers.MainTimerTcs.Task.IsCompleted)
                    {
                        timers.MainTimerTcs.TrySetResult(true);
                    }

                    if (!timers.StealTimerTcs.Task.IsCompleted)
                    {
                        timers.StealTimerTcs.TrySetResult(true);
                    }
                }

                var roles = Context.Items["Roles"] as List<string>;
                string guessingTeam = roles!.FirstOrDefault(r => r == Role.Blue.ToString() || r == Role.Red.ToString())!;

                //LOGIKA ZA SMANJIVANJE SCOREA STO JE MANJE VREME
                await _gameService.IncrementScoreAsync(gameId, guessingTeam, 100);

                //FRONTEND : slusa da bi updateovao UI i prikazuje kao u skribl
                await Clients.Group(gameId).SendAsync("ReceiveCorrectGuess", $"{guessingTeam} wins!", guessingTeam, guess);
            }
            else
            {
                //ZA TESTIRANJE SAMO
                await Clients.Caller.SendAsync("IncorrectGuess");
            }
        }
        public async Task EndRound(GameRound gameRound)
        {
            var gameKey = Context.Items["GameKey"]!.ToString();

            if(IsAdmin())
            {
                gameRound.GameState = GameState.RoundEnded;
                await _gameService.UpdateGameRoundAsync(gameRound);
                await Clients.Group(gameKey!).SendAsync("RoundEnded", gameRound);
                if (_gameTimers.TryRemove(gameKey!, out var timers))
                {
                    timers.MainTimerTcs.TrySetCanceled();
                    timers.StealTimerTcs.TrySetCanceled();
                }
            }

            var (isError, game, error) = await _gameService.GetGameAsync(gameKey!);
            if (isError) throw new HubException("Game not found : " + error);

            if (gameRound.CurrentRound < game!.TotalRounds)
            {
                await Clients.Group(gameKey!).SendAsync("StartingNextRound", true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            }
            else
            {
                //OVDE FRONTEND ucitava lepo ceo game model iz delova i prikazuje konacan score
                await Clients.Group(gameKey!).SendAsync("GameEnded", game, gameRound);

                var winningTeam = gameRound.BlueScore > gameRound.RedScore ? "Blue" : "Red";

                await Clients.Group(gameKey!).SendAsync("ReceiveGameWinner", winningTeam);

                // Additional cleanup, if necessary
                await _gameService.DeleteGameAsync(gameKey!);
            }
        }
        public override async Task OnConnectedAsync()
        {
            var sessionId = Context.GetHttpContext()!.Request.Cookies["UserSessionId"];
            var (isError, user, error) = await _userService.GetUserAsync(sessionId!);
            if (isError)
            {
                throw new HubException(error);
            }
            Context.Items["SeshKey"] = sessionId;
            Context.Items["Nickname"] = user!.Nickname;
            Context.Items["Roles"] = user.Roles;

            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if(!Context.Items.ContainsKey("GameKey"))
            {
                await base.OnDisconnectedAsync(exception);
            }
            var seshKey = Context.Items["SeshKey"]!.ToString();
            var gameKey = Context.Items["GameKey"]!.ToString();
            await _userService.RemoveRolesAsync(seshKey!, [Role.Red, Role.Blue, Role.Painter, Role.TeamLeader]);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameKey!);
            
            if(IsAdmin()) await _roomService.UpdateRoomStateAsync(gameKey!.Split(':')[1], RoomState.Waiting);

            await base.OnDisconnectedAsync(exception);
        }
        private bool IsAdmin()
        {
            var roles = Context.Items["Roles"] as List<string>;
            return roles != null && roles.Contains("RoomAdmin");
        }

        public async Task BroadcastDrawing(string gameId, string drawingData)
        {
            await Clients.Others.SendAsync("ReceiveDrawing", drawingData);
        }


    }
}
