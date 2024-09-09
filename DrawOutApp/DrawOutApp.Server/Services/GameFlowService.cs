
using System.Collections.Concurrent;

namespace DrawOutApp.Server.Services
{
    public class GameFlowService : BackgroundService
    {
        private readonly ConcurrentQueue<Func<Task>> _gameEvents = new ConcurrentQueue<Func<Task>>();
        
        public void EnqueueEvent(Func<Task> gameEvent)
        {
            _gameEvents.Enqueue(gameEvent);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_gameEvents.TryDequeue(out var gameEvent))
                {
                    await gameEvent();
                }

                await Task.Delay(100, stoppingToken); 
            }
        }
    }
}
