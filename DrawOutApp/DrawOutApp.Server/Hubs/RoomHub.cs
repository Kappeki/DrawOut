using DrawOutApp.Server.Entities;
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

        public RoomHub(IChatMessageRepo chatRepo, IUserService userService)
        {
            _chatRepo = chatRepo;
            _userService = userService;
        }

        public async Task JoinRoom(string roomId, string nickname)
        {
            var msg = new ChatMessage
            {
                Sender = "Server",
                Content = $"{nickname} has joined the room!",
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UserJoined", msg);
        }

        public async Task LeaveRoom(string roomId, string nickname)
        {
            var msg = new ChatMessage
            {
                Sender = "Server",
                Content = $"{nickname} has left the room!",
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UserLeft", msg);
        }

        public async Task SendMessageToRoom(string roomId, string message)
        {
            var httpContext = Context.GetHttpContext();

            if (httpContext == null)
            {
                await Clients.Caller.SendAsync("Error", "No HTTP context available!");
                return;
            }

            var seshKey = httpContext.Request.Cookies["UserSessionId"];
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
            await Clients.Group(roomId).SendAsync("ReceiveMessage", msg);
        }


        public override async Task OnConnectedAsync()
        {
            // Optional: handle logic when a user connects, if needed.
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Optional: handle logic when a user disconnects, if needed.
            await base.OnDisconnectedAsync(exception);
        }
    }
}
