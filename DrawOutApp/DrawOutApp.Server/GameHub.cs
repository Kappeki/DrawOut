
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace DrawOutApp.Server
{
    public class GameHub : Hub
    {
        public async Task BroadcastDrawing(string drawingData)
        {
            await Clients.Others.SendAsync("ReceiveDrawing", drawingData);
        }

        public async Task SendGuess(string guess)
        {
            
            await Clients.All.SendAsync("ReceiveGuess", guess, Context.ConnectionId);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
            await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
        }
    }
}
