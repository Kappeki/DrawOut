using AutoMapper;
using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Services.Contracts;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
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
        private readonly IDrawingActionService _drawingActionService;
        public GameHub(
            IGameService gameService, 
            IUserService userService,
            IRoomService roomService, 
            ITimerService timerService,
            IDrawingActionService drawingActionService
            )
        {
            _gameService = gameService;
            _userService = userService;
            _roomService = roomService;
            _timerService = timerService;
            _drawingActionService = drawingActionService;
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

        public async Task Draw(List<DrawingActionModel> actions)
        {
            var gameId = Context.Items["GameKey"]!.ToString();
            var seshKey = Context.Items["SeshKey"]!.ToString();
            await Clients.GroupExcept(gameId!, Context.ConnectionId).SendAsync("UpdateDrawing", actions);

            //var gameRoundModel = await _gameService.GetGameRoundAsync(gameId!);

            //await _drawingActionService.AddDrawingActionAsync(actionModel);
            
        }
        public async Task ClearBoard()
        {
            var gameId = Context.Items["GameKey"]!.ToString();
            var seshKey = Context.Items["SeshKey"]!.ToString();

            var gameRoundModel = await _gameService.GetGameRoundAsync(gameId!);
            if(gameRoundModel.CurrentPainter == seshKey)
            {
                var painterName = Context.Items["Nickname"]!.ToString();
               // await _drawingActionService.ClearDrawingActionsAsync(gameId!.Split(':')[1], painterName!);
                await Clients.GroupExcept(gameId!, Context.ConnectionId).SendAsync("ClearBoard", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            }
        }
        public async Task UndoLastStroke(string strokeId)
        {
            var gameId = Context.Items["GameKey"]!.ToString();
            await Clients.GroupExcept(gameId!, Context.ConnectionId).SendAsync("UndoAction", strokeId);

            //var gameRoundModel = await _gameService.GetGameRoundAsync(gameId!);

            //if (gameRoundModel.CurrentPainter == seshKey)
            //{
            //    var painterName = Context.Items["Nickname"]!.ToString();
            //    //await _drawingActionService.UndoStrokeAsync(gameId!.Split(':')[1], painterName!, strokeId);
            //}
        }

        public async Task SendMementoSave()
        {
            var gameId = Context.Items["GameKey"]!.ToString();
            await Clients.GroupExcept(gameId!, Context.ConnectionId).SendAsync("SaveMemento", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
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
            Context.Items["Roles"] = user!.Roles;

            await _userService.SetConnectionIdAsync(sessionId!, Context.ConnectionId, TimeSpan.FromDays(7));

            await base.OnConnectedAsync();
        }
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

    }

}

