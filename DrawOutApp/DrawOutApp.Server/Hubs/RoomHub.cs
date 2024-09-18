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

        private async Task<bool> TryJoinRoom(string roomId, string nickname, string? password, bool urlJoin = false)
        {
            var seshKey = Context.Items["SeshKey"]!.ToString();
            var roles = Context.Items["Roles"] as List<string>;
            Context.Items["RoomId"] = roomId;
            if (roles != null && roles.Count > 0)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "Server", "You cannot join multiple rooms simultaneously.", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                return false;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            
            await Clients.Group(roomId).SendAsync("ReceiveMessage", "Server", $"{nickname} has joined the room.", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            
            var (isError, success, error) = await _roomService.AddPlayerAsync(roomId, seshKey!, password, urlJoin);
            if (isError)
            {
                throw new HubException(error);
            }

            await SendConnectedRoomById(roomId, true);
            
            await SendConnectedUsers(roomId);
            
            return true;
        }
        public async Task JoinRoomById(string roomId, string? password = null)
        {
            if (!await TryJoinRoom(roomId, Context.Items["Nickname"]!.ToString()!, password))
            {
                return;
            }
        }
        public async Task JoinRoomByURL(string roomURL)
        {
            var roomId = await _roomService.GetIdFromURL(roomURL) ?? throw new HubException("Room not found.");
            if (!await TryJoinRoom(roomId, Context.Items["Nickname"]!.ToString()!, null, true))
            {
                return;
            }
        }
        public async Task SendConnectedUsers(string roomId)
        {
            var (isError, users, error) = await _roomService.GetPlayerIdsAsync(roomId);

            if (users == null)
                return;

            if (isError)
                throw new HubException(error);

            var usersInfo = new List<UserModel>();
            foreach (var userId in users!)
            {
                await _userService.AddRolesAsync(userId, [Role.Player]);
                var (userIsError, user, userError) = await _userService.GetUserAsync(userId);
                if (userIsError)
                {
                    throw new HubException(userError);
                }

                if(userId == Context.Items["SeshKey"]!.ToString()!)
                {
                    var userRoles = Context.Items["Roles"] as List<string>;
                    userRoles?.Add("Player");
                    Context.Items["Roles"] = userRoles;
                }

                usersInfo.Add(user!);
            }

            await Clients
                .Group(roomId)
                .SendAsync("ConnectedUsers", usersInfo);
        }
        public async Task SendConnectedRoomById(string roomId, bool isJoining = false)
        {
            var (isError,room,error) = await _roomService.GetRoomByIdAsync(roomId);
            if(isError)
            {
                throw new HubException(error);
            }
            if(room!.RoomAdminId == Context.Items["SeshKey"]!.ToString() && isJoining)
            {
                Context.Items["Roles"] = new List<string> { "RoomAdmin" };
                await _userService.AddRolesAsync(Context.Items["SeshKey"]!.ToString()!, [Role.RoomAdmin]);
            }

            await Clients
                .Group(roomId)
                .SendAsync("ConnectedRoom", room);
        }
        public async Task SendMessageToRoom(string roomUrl, string message)
        {
            var seshKey = Context.Items["SeshKey"]!.ToString();
            var user = await _userService.GetUserAsync(seshKey!);
            var roomId = await _roomService.GetIdFromURL(roomUrl);

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
           
            await _chatRepo.AddToChatAsync(roomId!, msg);
            await Clients
                .Group(roomId!)
                .SendAsync("ReceiveMessage", 
                msg.Sender, 
                msg.Content, 
                msg.Timestamp);
        }
        public async Task UpdateRoomState(string roomUrl, string roomState)
        {
            var roomId = await _roomService.GetIdFromURL(roomUrl);
            var (isError, room, error) = await _roomService.GetRoomByUrlAsync(roomUrl);
            if (isError)
            {
                throw new HubException(error);
            }
            if(room!.RoomAdminId != Context.Items["SeshKey"]!.ToString())
            {
                throw new HubException("You do not have permission to change room state.");
            }

            await _roomService.UpdateRoomStateAsync(roomId!, Enum.Parse<RoomState>(roomState));

            await SendConnectedRoomById(roomId!);
            await SendConnectedUsers(roomId!);
        }
        public async Task ChangeRoomSettings(string roomURL, string settingName, string settingValue)
        {
            var roles = Context.Items["Roles"] as List<string>;
            if (roles == null || !roles.Contains("RoomAdmin"))
            {
                throw new HubException("You do not have permission to change room settings.");
            }
            var (isError, roomModel, error) = await _roomService.GetRoomByUrlAsync(roomURL);
            switch (settingName)
            {
                case "CustomWords":
                    roomModel!.CustomWords = settingValue.Split(',').ToList();
                    break;
                case "SelectedWordPack":
                    roomModel!.SelectedWordPack = settingValue;
                    break;
                case "RoundTime":
                    if (Enum.TryParse(settingValue, out RoundTime roundTime))
                    {
                        roomModel!.RoundTime = (int)roundTime;
                    }
                    break;
                default:
                    throw new HubException("Invalid setting name.");
            }

            var roomId = Context.Items["RoomId"]!.ToString();

            await _roomService.UpdateRoomAsync(roomModel!);

            await Clients
                .Group(roomId!)
                .SendAsync("RoomSettingsChanged", settingName, settingValue);
        }
        public async Task SwitchTeam(string roomUrl, string? oldTeam, string newTeam)
        {
            var nickname = Context.Items["Nickname"]!.ToString();
            var roomId = await _roomService.GetIdFromURL(roomUrl);

            if (!string.IsNullOrEmpty(oldTeam))
            {
                if (oldTeam == "Red")
                {
                    await _userService.RemoveRolesAsync(Context.Items["SeshKey"]!.ToString()!, [Role.Red]);
                }
                else if (oldTeam == "Blue")
                {
                    await _userService.RemoveRolesAsync(Context.Items["SeshKey"]!.ToString()!, [Role.Blue]);
                }
            }
            if (newTeam == "Red")
            {
                await _userService.AddRolesAsync(Context.Items["SeshKey"]!.ToString()!, [Role.Red]);
            }
            else if (newTeam == "Blue")
            {
                await _userService.AddRolesAsync(Context.Items["SeshKey"]!.ToString()!, [Role.Blue]);
            }
            else
            {
                throw new HubException("Invalid team name.");
            }

            await Clients.Group(roomId!).SendAsync("ReceiveTeamSwitch", oldTeam, newTeam, nickname);
        }
        public override async Task OnConnectedAsync()
        {
            var (isError, user, error) = await _userService.GetUserSessionAsync(Context.GetHttpContext()!.Request);
            if (isError)
            {
                throw new HubException(error);
            }
            Context.Items["SeshKey"] = user!._sessionKey;
            Context.Items["Nickname"] = user!.Nickname;
            Context.Items["Roles"] = user!.Roles;

            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if(!Context.Items.ContainsKey("RoomId"))
            {
                await base.OnDisconnectedAsync(exception);
            }
            
            var seshKey = Context.Items["SeshKey"]!.ToString();
            var roomId = Context.Items["RoomId"]!.ToString();

            var (isError, userIds, error) = await _roomService.GetPlayerIdsAsync(Context.Items["RoomId"]!.ToString()!);
            var isProtected = await _roomService.CheckPasswordProtection(roomId!);

            if (userIds != null)
            {
                if (AdminDisconnected() && userIds!.Count > 1 && !isProtected)
                {
                    Random random = new Random();
                    var newAdmin = userIds![random.Next(userIds!.Count)];
                    await _userService.AddRolesAsync(newAdmin, [Role.RoomAdmin]);
                    await _userService.RemoveRolesAsync(seshKey!, [Role.RoomAdmin]);
                    var user = await _userService.GetUserAsync(newAdmin);
                    var success = await _roomService.OnAdminDisconnectedAsync(roomId!, newAdmin);

                    if (success)
                    {
                        await Clients.Group(roomId!).SendAsync(
                            "ReceiveMessage",
                            "Server",
                            $"{user.Data!.Nickname} is the new admin!", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    }
                    else
                    {
                        throw new HubException("Couldn't set new admin");
                    }


                }
                userIds!.Remove(seshKey!);
            }

            await _userService.RemoveRolesAsync(seshKey!, [Role.RoomAdmin, Role.Player, Role.Red, Role.Blue, Role.Painter, Role.TeamLeader]);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId!);

            var isRemoved = await _roomService.RemovePlayerAsync(roomId!, seshKey!);
            if(!isRemoved.Data)
            {
                await base.OnDisconnectedAsync(exception);
            }

            if (userIds?.Count >= 1)
            {
                await Clients
                    .Group(roomId!)
                    .SendAsync("ReceiveMessage", "Server", $"{Context.Items["Nickname"]!.ToString()} has left the room.", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                await SendConnectedRoomById(roomId!);
                await SendConnectedUsers(roomId!);
            }
            
            await _roomService.SetRoomExpirationAsync(roomId!);
            await base.OnDisconnectedAsync(exception);
        }
        private bool AdminDisconnected()
        {
            var roles = Context.Items["Roles"] as List<string>;
            return roles != null && roles.Contains("RoomAdmin");
        }
    }
}
