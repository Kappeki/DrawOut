
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace DrawOutApp.Server
{
    public class GameHub : Hub
    {
        // This method can be called by a client to broadcast a drawing to other clients
        public async Task BroadcastDrawing(string drawingData)
        {
            // Clients.Others means all clients except the caller will receive the drawing data
            await Clients.Others.SendAsync("ReceiveDrawing", drawingData);
        }

        // This method can be called by a client to send a guess to the server
        public async Task SendGuess(string guess)
        {
            // You can add logic here to check the guess, update scores, etc.
            // For example, broadcasting the guess to all clients
            await Clients.All.SendAsync("ReceiveGuess", guess, Context.ConnectionId);
        }

        // Override the OnConnectedAsync method to handle when a client connects to the hub
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            // You can add your own connection logic here, such as notifying clients of a new user
            await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
        }

        // Override the OnDisconnectedAsync method to handle when a client disconnects from the hub
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
            // Add your own disconnection logic here, such as notifying clients of a user leaving
            await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
        }
    }
}
