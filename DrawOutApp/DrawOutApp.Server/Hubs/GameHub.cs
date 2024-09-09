using AutoMapper;
using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Services.Contracts;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DrawOutApp.Server.Hubs
{
    public class GameHub : Hub
    {
        private readonly IGameService _gameService;
        private readonly IUserService _userService;
        private readonly IRoomService _roomService;
        private readonly ITimerService _timerService;
        public GameHub(
            IGameService gameService, 
            IUserService userService,
            IRoomService roomService, 
            ITimerService timerService
            )
        {
            _gameService = gameService;
            _userService = userService;
            _roomService = roomService;
            _timerService = timerService;
        }

        public async Task ConnectToGame(string gameId)
        {
            var seshKey = Context.Items["SeshKey"]!.ToString() ?? throw new HubException("Session ID is missing.");
            Context.Items["GameKey"] = gameId;
            Context.Items["RoomId"] = gameId.Split(':')[1];

            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
            
            await Clients.Caller.SendAsync("UserConnected", Context.Items["Nickname"]!.ToString());
        }

        public async Task StartGame()
        {
            var gameId = Context.Items["GameKey"]!.ToString();
            var gameTimers = _timerService.GetOrAdd(gameId!);
            await _gameService.StartGameAsync(gameId!, gameTimers);
        }

        public async Task SelectWord(string word)
        {
            var gameId = Context.Items["GameKey"]!.ToString();
            if(_timerService.TryGetValue(gameId!, out var timers)) 
            {
                await _gameService.SelectWordAsync(gameId!, word, timers);
            }
        }

        public async Task SubmitGuess(string guess)
        {
            var gameId = Context.Items["GameKey"]!.ToString();
            var seshKey = Context.Items["SeshKey"]!.ToString();
            if (_timerService.TryGetValue(gameId!, out var timers))
            {
                await _gameService.SubmitGuessAsync(seshKey!, gameId!, guess, timers);
            }
        }


        ////admin only
        //public async Task StartGame(GameModel gameModel)
        //{
        //    var gameId = await _gameService.CreateGameAsync(gameModel);
        //    gameModel._id = gameId;
        //    var gameRoundModel = new GameRoundModel
        //    {
        //        _gameId = gameId,
        //        GameState = GameState.WaitingForPlayers.ToString(),
        //        BlueScore = 0,
        //        RedScore = 0,
        //        CurrentPainter = gameModel.PainterOrder![0],
        //        CurrentRound = 0,
        //        SelectedWord = ""
        //    };

        //    var blueLeader = gameModel.TeamLeaders!["Blue"];
        //    var redLeader = gameModel.TeamLeaders!["Red"];
        //    //neka ostane da server handluje team leader logiku
        //    await _userService.AddRolesAsync(blueLeader, [Role.TeamLeader]);
        //    await _userService.AddRolesAsync(redLeader, [Role.TeamLeader]);


        //    _gameTimers[gameId] = (new TaskCompletionSource<bool>(), new TaskCompletionSource<bool>(), new TaskCompletionSource<bool>());

        //    await Clients.Group(gameId).SendAsync("LoadGameView", gameModel, gameRoundModel);

        //    await Clients.Caller.SendAsync("SendInitNextRound", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        //}
        //public async Task InitNextRound(GameRoundModel gameRoundModel)
        //{
        //    var gameKey = Context.Items["GameKey"]!.ToString();

        //    var (isError, game, error) = await _gameService.GetGameAsync(gameKey!);
        //    if (isError) throw new HubException("Game not found : " + error);

        //    await _userService.AddRolesAsync(gameRoundModel.CurrentPainter!, [Role.Painter]);

        //    gameRoundModel.CurrentRound++;
        //    gameRoundModel.GameState = GameState.Standby.ToString();
        //    var (gError, success, gerror) = await _gameService.UpdateGameRoundAsync(gameRoundModel);
        //    if (!success) throw new HubException("Error updating game round!");

        //    await Clients.Group(gameKey!).SendAsync("RoundStandby", gameRoundModel); 
        //}
        //public async Task EndRound(GameRoundModel gameRound)
        //{
        //    var gameKey = Context.Items["GameKey"]!.ToString();
        //    var (isError, game, error) = await _gameService.GetGameAsync(gameKey!);
        //    if (isError) throw new HubException("Game not found : " + error);

        //    gameRound.GameState = GameState.RoundEnded.ToString();
        //    await _userService.RemoveRolesAsync(gameRound.CurrentPainter!, [Role.Painter]);
        //    gameRound.CurrentPainter = game!.PainterOrder![gameRound.CurrentRound];
        //    await _gameService.UpdateGameRoundAsync(gameRound);
        //    await Clients.Group(gameKey!).SendAsync("RoundEnded", gameRound);

        //    if (gameRound.CurrentRound < game!.TotalRounds)
        //    {
        //        await Clients.Caller.SendAsync("SendInitNextRound", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        //    }
        //    else
        //    {
        //        //OVDE FRONTEND ucitava lepo ceo game model iz delova i prikazuje konacan score
        //        await Clients.Group(gameKey!).SendAsync("GameEnded", game, gameRound);

        //        var winningTeam = gameRound.BlueScore > gameRound.RedScore ? "Blue" : "Red";

        //        await Clients.Group(gameKey!).SendAsync("ReceiveGameWinner", winningTeam);

        //        // Additional cleanup, if necessary
        //        await _gameService.DeleteGameAsync(gameKey!);
        //    }
        //}
        //public async Task HandleNoWordSelected(List<string> selectables)
        //{
        //    var gameKey = Context.Items["GameKey"]!.ToString();

        //    // Notify clients that no word was selected
        //    await Clients.Group(gameKey!).SendAsync("WordSelected", selectables[0].Length);

        //    await _gameService.SetWordAsync(gameKey!, selectables[0]);

        //    // Ensure the word selection timer is cleaned up properly
        //    if (_gameTimers.TryGetValue(gameKey!, out var timers))
        //    {
        //        if (!timers.WordSelectionTcs.Task.IsCompleted)
        //        {
        //            timers.WordSelectionTcs.TrySetResult(false);
        //        }
        //    }

        //    await Clients.Group(gameKey!).SendAsync("SendStartRound", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        //}

        ////svi zovu
        //public async Task StartWordSelection(string currentPainterId)
        //{
        //    var gameKey = Context.Items["GameKey"]!.ToString();
        //    //dodatna provera
        //    if (Context.Items["SeshKey"]!.ToString() == currentPainterId)
        //    {
        //        var nickname = Context.Items["Nickname"]!.ToString();
        //        await Clients.Caller.SendAsync("PromptWordSelect", true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        //        await Clients.GroupExcept(gameKey!, Context.ConnectionId).SendAsync("SendPainterSelecting", nickname, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        //    }
        //    else
        //    {
        //        await Clients.Caller.SendAsync("PromptWordSelect", false, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        //    }
        //}
        //public async Task SelectWord(string gameId, string selectedWord)
        //{
        //    if (_gameTimers.TryGetValue(gameId, out var timers))
        //    {
        //        await _gameService.SetWordAsync(gameId, selectedWord);

        //        await Clients.Group(gameId).SendAsync("WordSelected", selectedWord.Length);

        //        // Stop the Word Selection Timer
        //        timers.WordSelectionTimer?.Dispose();
        //        timers.WordSelectionTimer = null;
        //    }
        //}

        //public async Task StartRound(GameRoundModel gameRound)
        //{
        //    var gameKey = Context.Items["GameKey"]!.ToString();

        //    if (IsAdmin())
        //    {
        //        gameRound.GameState = GameState.InProgress.ToString();
        //        await _gameService.UpdateGameRoundAsync(gameRound);
        //        await Clients.Group(gameKey!).SendAsync("RoundInProgress", gameRound);
        //    }

        //    var (isError, painter, error) = await _userService.GetUserAsync(gameRound.CurrentPainter!);
        //    var roles = Context.Items["Roles"] as List<string>;
        //    var painterTeam = painter!.Roles!.FirstOrDefault(r => r == Role.Blue.ToString() || r == Role.Red.ToString())!;

        //    if (Context.Items["SeshKey"]!.ToString() == gameRound.CurrentPainter)
        //    {
        //        await Clients.Caller.SendAsync("EnableDrawing", true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        //        await Clients.Caller.SendAsync("EnableGuessing", false, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        //        await Clients.GroupExcept(gameKey!,Context.ConnectionId).SendAsync("EnableDrawing", false, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        //    }
        //    else
        //    {
        //        if(roles!.Contains(painterTeam))
        //        {
        //            await Clients.Caller.SendAsync("EnableGuessing", true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        //        }
        //    }
        //}
        //public async Task StartRoundSteal(GameRoundModel gameRound)
        //{
        //    var gameKey = Context.Items["GameKey"]!.ToString();

        //    if (IsAdmin())
        //    {
        //        gameRound.GameState = GameState.Steal.ToString();
        //        await _gameService.UpdateGameRoundAsync(gameRound);
        //        await Clients.Group(gameKey!).SendAsync("RoundSteal", gameRound);
        //    }

        //    var (isError, painter, error) = await _userService.GetUserAsync(gameRound.CurrentPainter!);
        //    if (isError) throw new HubException("Painter not found : " + error);

        //    var painterTeam = painter!.Roles!.FirstOrDefault(r => r == Role.Blue.ToString() || r == Role.Red.ToString())!;

        //    var roles = Context.Items["Roles"] as List<string>;

        //    if (Context.Items["SeshKey"]!.ToString() == gameRound.CurrentPainter)
        //    {
        //        await Clients.Caller.SendAsync("EnableDrawing", false, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        //    }

        //    if (roles!.Contains(Role.TeamLeader.ToString()) && !roles.Contains(painterTeam))
        //    { 
        //        await Clients.GroupExcept(gameKey!, Context.ConnectionId).SendAsync("EnableGuessing", false, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        //        await Clients.Caller.SendAsync("EnableGuessing", true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        //    }
        //}
        //public async Task SubmitGuess(string gameId, string guess)
        //{
        //    var isCorrect = await _gameService.CheckGuessAsync(gameId, guess);
        //    if (isCorrect)
        //    {
        //        if (_gameTimers.TryGetValue(gameId, out var timers))
        //        {
        //            timers.MainTimer?.Dispose();
        //            timers.MainTimer = null;

        //            timers.StealTimer?.Dispose();
        //            timers.StealTimer = null;
        //        }

        //        var roles = Context.Items["Roles"] as List<string>;
        //        string guessingTeam = roles!.FirstOrDefault(r => r == Role.Blue.ToString() || r == Role.Red.ToString())!;

        //        //LOGIKA ZA SMANJIVANJE SCOREA STO JE MANJE VREME
        //        await _gameService.IncrementScoreAsync(gameId, guessingTeam, 100);

        //        //FRONTEND: slusa da bi updateovao UI i prikazuje kao u skribl
        //        await Clients.Group(gameId).SendAsync("ReceiveCorrectGuess", $"{guessingTeam} wins!", guessingTeam, guess);
        //    }
        //    else
        //    {
        //        //ZA TESTIRANJE SAMO
        //        await Clients.Caller.SendAsync("IncorrectGuess");
        //    }
        //}

        //public async void StartSelectTimer()
        //{
        //    var gameKey = Context.Items["GameKey"]!.ToString();
        //    if (_gameTimers.TryGetValue(gameKey!, out var timers))
        //    {
        //        int remainingTime = 15;
        //        bool wordSelected = false;

        //        var timer = new Timer(async state =>
        //        {
        //            if (wordSelected || remainingTime <= 0)
        //            {
        //                timers.WordSelectionTimer?.Dispose();
        //                timers.WordSelectionTimer = null;
        //                return;
        //            }

        //            remainingTime--;

        //            await Clients.Group(gameKey!).SendAsync("UpdateWordSelectTimer", remainingTime);
        //        }, null, 1000, 1000);

        //        timers.WordSelectionTimer = timer;

        //        // Simulate a word selection scenario
        //        await Task.Delay(remainingTime * 1000); // Wait for the selection period
        //    }
        //}

        //public async void StartRoundTimer()
        //{
        //    var gameKey = Context.Items["GameKey"]!.ToString();

        //    var (isError, gameModel, error) = await _gameService.GetGameAsync(gameKey!);
        //    if (isError) throw new HubException("Game not found: " + error);

        //    if (!_gameTimers.TryGetValue(gameKey!, out var timers))
        //    {
        //        timers = new GameTimers();
        //        _gameTimers[gameKey!] = timers;
        //    }

        //    int remainingTime = gameModel!.MainTimer;
        //    bool correctGuess = false;

        //    var timer = new Timer(async state =>
        //    {
        //        if (correctGuess || remainingTime <= 0)
        //        {
        //            timers.MainTimer?.Dispose();
        //            timers.MainTimer = null;
        //            return;
        //        }

        //        remainingTime--;

        //        await Clients.Group(gameKey!).SendAsync("UpdateMainTimer", remainingTime);
        //    }, null, 1000, 1000);

        //    timers.MainTimer = timer;

        //    // Simulate the round timer
        //    await Task.Delay(remainingTime * 1000); // Wait for the timer duration
        //}

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

            await _userService.SetConnectionIdAsync(sessionId!, Context.ConnectionId, TimeSpan.FromDays(7));

            await base.OnConnectedAsync();
        }
        //ovo se poziva kad se zatvori game component ili disabluje?
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (!Context.Items.ContainsKey("GameKey"))
            {
                await base.OnDisconnectedAsync(exception);
                return;
            }

            var seshKey = Context.Items["SeshKey"]!.ToString();
            var gameKey = Context.Items["GameKey"]!.ToString();
            
            if(seshKey != null)
            {
                await _userService.RemoveRolesAsync(seshKey, [Role.Red, Role.Blue, Role.Painter, Role.TeamLeader]);
                await Groups.RemoveFromGroupAsync(seshKey, gameKey!);
            }

            if(IsAdmin()) await _roomService.UpdateRoomStateAsync(gameKey!.Split(':')[1], RoomState.Waiting);

            await base.OnDisconnectedAsync(exception);
        }
        private bool IsAdmin()
        {
            var roles = Context.Items["Roles"] as List<string>;
            return roles != null && roles.Contains("RoomAdmin");
        }

        ////public async void StartSelectTimer()
        ////{
        ////    var gameKey = Context.Items["GameKey"]!.ToString();
        ////    if (_gameTimers.TryGetValue(gameKey!, out var timers))
        ////    {
        ////        var wordSelectionTcs = new TaskCompletionSource<bool>();

        ////        _gameTimers[gameKey!] = (MainTimerTcs: _gameTimers[gameKey!].MainTimerTcs,
        ////            StealTimerTcs: _gameTimers[gameKey!].StealTimerTcs,
        ////            WordSelectionTcs: wordSelectionTcs);

        ////        int remainingTime = 15;

        ////        var timer = new Timer(async state =>
        ////        {
        ////            remainingTime--;

        ////            await Clients.Group(gameKey!).SendAsync("UpdateWordSelectTimer", remainingTime);

        ////            if (_gameTimers.TryGetValue(gameKey!, out var timers))
        ////            {
        ////                if (!timers.WordSelectionTcs.Task.IsCompleted)
        ////                {
        ////                    if (remainingTime <= 0)
        ////                    {
        ////                        timers.WordSelectionTcs.TrySetResult(false);
        ////                    }
        ////                }
        ////            }
        ////        }, null, 1000, 1000); // 1-second intervals

        ////        var wordSelected = await wordSelectionTcs.Task;
        ////        timer.Dispose();

        ////        if (wordSelected)
        ////        {
        ////            await Clients.Group(gameKey!).SendAsync("SendStartRound", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        ////        }
        ////        else
        ////        {
        ////            await Clients.Caller.SendAsync("SendHandleNoWord", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        ////        }
        ////    }
        ////}
        ////public async void StartRoundTimer()
        ////{
        ////    var roundTcs = new TaskCompletionSource<bool>();
        ////    var gameKey = Context.Items["GameKey"]!.ToString();

        ////    var (isError, gameModel, error) = await _gameService.GetGameAsync(gameKey!);
        ////    if(isError) throw new HubException("Game not found : " + error);

        ////    _gameTimers[gameKey!] = (MainTimerTcs: roundTcs,
        ////        StealTimerTcs: _gameTimers[gameKey!].StealTimerTcs,
        ////        WordSelectionTcs: _gameTimers[gameKey!].WordSelectionTcs);

        ////    int remainingTime = gameModel!.MainTimer;

        ////    var timer = new Timer(async state =>
        ////    {
        ////        remainingTime--;

        ////        await Clients.Group(gameKey!).SendAsync("UpdateMainTimer", remainingTime);

        ////        if (_gameTimers.TryGetValue(gameKey!, out var timers))
        ////        {
        ////            if (!timers.MainTimerTcs.Task.IsCompleted)
        ////            {
        ////                if (remainingTime <= 0)
        ////                {
        ////                    timers.MainTimerTcs.TrySetResult(false);
        ////                }
        ////            }
        ////        }
        ////    }, null, 1000, 1000); // 1-second intervals

        ////    var correctGuess = await roundTcs.Task;
        ////    timer.Dispose();

        ////    if (correctGuess)
        ////    {
        ////        await Clients.Group(gameKey!).SendAsync("SendEndRound", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        ////    }
        ////    else
        ////    {
        ////        await Clients.Group(gameKey!).SendAsync("SendStartSteal", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        ////    }
        ////}
        ////public async void StartStealTimer()
        ////{
        ////    var stealTcs = new TaskCompletionSource<bool>();
        ////    var gameKey = Context.Items["GameKey"]!.ToString();

        ////    var (isError, gameModel, error) = await _gameService.GetGameAsync(gameKey!);
        ////    if(isError) throw new HubException("Game not found : " + error);

        ////    _gameTimers[gameKey!] = (MainTimerTcs: _gameTimers[gameKey!].MainTimerTcs,
        ////        StealTimerTcs: stealTcs,
        ////        WordSelectionTcs: _gameTimers[gameKey!].WordSelectionTcs);

        ////    int remainingTime = gameModel!.StealTimer;

        ////    var timer = new Timer(async state =>
        ////    {
        ////        remainingTime--;

        ////        await Clients.Group(gameKey!).SendAsync("UpdateStealTimer", remainingTime);

        ////        if (_gameTimers.TryGetValue(gameKey!, out var timers))
        ////        {
        ////            if (!timers.StealTimerTcs.Task.IsCompleted)
        ////            {
        ////                if (remainingTime <= 0)
        ////                {
        ////                    timers.StealTimerTcs.TrySetResult(false);
        ////                }
        ////            }
        ////        }
        ////    }, null, 1000, 1000); // 1-second intervals

        ////    await stealTcs.Task;
        ////    timer.Dispose();

        ////    await Clients.Group(gameKey!).SendAsync("SendEndRound", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        ////}
        ////public async void StopTimers()
        ////{
        ////    var gameKey = Context.Items["GameKey"]!.ToString();
        ////    if (_gameTimers.TryRemove(gameKey!, out var timers))
        ////    {
        ////        timers.MainTimerTcs.TrySetCanceled();
        ////        timers.StealTimerTcs.TrySetCanceled();
        ////    }
        ////    _gameTimers[gameKey!] = (new TaskCompletionSource<bool>(), new TaskCompletionSource<bool>(), new TaskCompletionSource<bool>());
        ////    await Clients.Caller.SendAsync("TimersReset", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        ////}
        //public async Task BroadcastDrawing(string gameId, string drawingData)
        //{
        //    await Clients.Others.SendAsync("ReceiveDrawing", drawingData);
        //}
    }

}
