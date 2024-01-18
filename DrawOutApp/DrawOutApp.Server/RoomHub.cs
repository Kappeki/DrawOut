using DrawOutApp.Server.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace DrawOutApp.Server
{
    public class RoomHub : Hub
    {
        // Method for clients to call to join a room

        private readonly IDistributedCache _cache;

        public RoomHub(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task JoinRoom(string roomId, string nickname)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UserJoined", $"{nickname} has joined the room!");
        }
        public async Task LeaveRoom(string roomId, string nickname)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UserLeft", $"{nickname} has left the room!");
        }

        public async Task SendMessageToRoom(string roomId, string message, string nickname)
        {
            var chatMessage = new ChatMessage
            {
                Sender = nickname,
                Content = message,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            var serializedMsg = JsonConvert.SerializeObject(chatMessage);
            var msgKey = $"chat:{roomId}:{chatMessage.Timestamp}";
            await _cache.SetStringAsync(msgKey, serializedMsg);


            await Clients.Group(roomId).SendAsync("ReceiveMessage", nickname, message);
        }

        // Override the OnConnectedAsync method to handle what happens when a user connects to the Hub
        public override async Task OnConnectedAsync()
        {
            // Optional: handle logic when a user connects, if needed.
            await base.OnConnectedAsync();
        }

        // Override the OnDisconnectedAsync method to handle what happens when a user disconnects from the Hub
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Optional: handle logic when a user disconnects, if needed.
            await base.OnDisconnectedAsync(exception);
        }
    }
}
