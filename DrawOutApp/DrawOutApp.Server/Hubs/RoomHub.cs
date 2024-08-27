using AutoMapper;
using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Services.Contracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace DrawOutApp.Server.Hubs
{
    public class RoomHub : Hub
    {
        // Method for clients to call to join a room
        private readonly IChatMessageRepo _chatRepo;
        private readonly IUserService _userService;
        private readonly IRoomService _roomService;
        private readonly IMapper _mapper;
        public RoomHub(IChatMessageRepo chatRepo, IUserService userService, IRoomService roomService, IMapper mapper)
        {
            _chatRepo = chatRepo;
            _userService = userService;
            _roomService = roomService;
            _mapper = mapper;
        }
        private async Task<bool> TryJoinRoom(string roomId, string nickname)
        {
            Context.Items["RoomId"] = roomId;
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("ReceiveMessage", $"{nickname} has joined the room.");
            await SendConnectedRoomById(roomId);
            await SendConnectedUsers(roomId);
            return true;
        }
        public async Task JoinRoomById(string roomId, string? password = null)
        {
            var seshKey = Context.Items["SeshKey"]!.ToString();
            if (!await TryJoinRoom(roomId, Context.Items["Nickname"]!.ToString()!))
            {
                return;
            }
            await _roomService.AddPlayerAsync(roomId, seshKey!, password);

        }
        public async Task JoinRoomByURL(string roomURL, string? password = null)
        {
            var roomId = await _roomService.GetIdFromURL(roomURL) ?? throw new HubException("Room not found.");
            if (!await TryJoinRoom(roomId, Context.Items["Nickname"]!.ToString()!))
            {
                return;
            }
            await _roomService.AddPlayerAsync(roomId, Context.Items["SeshKey"]!.ToString()!, password);
        }
        public async Task SendConnectedUsers(string roomId)
        {
            var (isError, users, error) = await _roomService.GetPlayerIdsAsync(roomId);
            if (isError)
                throw new HubException(error);

            var usersInfo = new List<PlayerInfo>();
            foreach (var userId in users!)
            {
                await _userService.AddRolesAsync(userId, [Role.Player]);
                usersInfo.Add(_mapper.Map<PlayerInfo>(await _userService.GetUserAsync(userId)));
            }

            await Clients
                .Group(roomId)
                .SendAsync("ConnectedUsers", usersInfo);
        }
        public async Task SendConnectedRoomById(string roomId)
        {
            var (isError,room,error) = await _roomService.GetRoomByIdAsync(roomId);
            if(isError)
            {
                throw new HubException(error);
            }
            if(room!.RoomAdminId == Context.Items["SeshKey"]!.ToString())
            {
                await _userService.AddRolesAsync(Context.Items["SeshKey"]!.ToString()!, [Role.RoomAdmin]);
            }

            await Clients
                .Group(roomId)
                .SendAsync("ConnectedRoom", room);
        }
        public async Task SendMessageToRoom(string roomId, string message)
        {
            var seshKey = Context.Items["SeshKey"]!.ToString();
            var user = await _userService.GetUserAsync(seshKey!);

            if(user.IsError)
            {
                await Clients.Caller.SendAsync("Error", "User not found.");
                return;
            }

            var msg = new ChatMessage
            {
                Sender = user.Data!.Nickname,
                Content = message,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
           
            await _chatRepo.AddToChatAsync(roomId, msg);
            await Clients
                .Group(roomId)
                .SendAsync("ReceiveMessage", 
                msg.Sender, 
                msg.Content, 
                msg.Timestamp);
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

            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if(!Context.Items.ContainsKey("RoomId"))
            {
                await base.OnDisconnectedAsync(exception);
            }

            var roomId = Context.Items["RoomId"]!.ToString();
            var seshKey = Context.Items["SeshKey"]!.ToString();

            var isRemoved = await _roomService.RemoveUserAsync(roomId!, seshKey!);
            if(!isRemoved.Data)
            {
                await Clients.Caller.SendAsync("Error", isRemoved.Error);
                await base.OnDisconnectedAsync(exception);
            }

            await _userService.RemoveRolesAsync(seshKey!, [Role.RoomAdmin, Role.Player, Role.Red, Role.Blue, Role.Painter, Role.TeamLeader]);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId!);
            await Clients
                .Group(roomId!)
                .SendAsync("ReceiveMessage", "System", $"{Context.Items["Nickname"].ToString()} has left the room.");

            await SendConnectedRoomById(roomId!);
            await SendConnectedUsers(roomId!);
            await base.OnDisconnectedAsync(exception);
        }
        public async Task ChangeRoomSettings(string roomId, string settingName, string settingValue)
        {
            await Clients
                .Group(roomId)
                .SendAsync("RoomSettingsChanged", settingName, settingValue);
        }
        public async Task JoinTeam(string roomId, string teamName)
        {
            var nickname = Context.Items["Nickname"]!.ToString();
            await Clients.Group(roomId).SendAsync("ReceiveTeamJoin", teamName, nickname);
            if (teamName == "Red")
            {
                await _userService.AddRolesAsync(Context.Items["SeshKey"]!.ToString()!, [Role.Red]);
            }
            else if (teamName == "Blue")
            {
                await _userService.AddRolesAsync(Context.Items["SeshKey"]!.ToString()!, [Role.Blue]);
            }
            else
            {
                throw new HubException("Invalid team name.");
            }
        }
        public async Task SwitchTeam(string roomId, string oldTeam, string newTeam)
        {
            var nickname = Context.Items["Nickname"]!.ToString();
            await Clients.Group(roomId).SendAsync("ReceiveTeamSwitch", oldTeam, newTeam, nickname);
            if (oldTeam == "Red")
            {
                await _userService.RemoveRolesAsync(Context.Items["SeshKey"]!.ToString()!, [Role.Red]);
                await _userService.AddRolesAsync(Context.Items["SeshKey"]!.ToString()!, [Role.Blue]);
            }
            else if (oldTeam == "Blue")
            {
                await _userService.RemoveRolesAsync(Context.Items["SeshKey"]!.ToString()!, [Role.Blue]);
                await _userService.AddRolesAsync(Context.Items["SeshKey"]!.ToString()!, [Role.Red]);
            }
            else
            {
                throw new HubException("Invalid team name.");
            }
        }
    }
}
