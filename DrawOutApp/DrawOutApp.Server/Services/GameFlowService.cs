
using System.Collections.Concurrent;

namespace DrawOutApp.Server.Services
{
    public class GameFlowService : BackgroundService
    {
        private readonly ConcurrentQueue<Func<Task>> _gameEvents = new ConcurrentQueue<Func<Task>>();

        private readonly ILogger<GameFlowService> _logger;

        public GameFlowService(ILogger<GameFlowService> logger)
        {
            _logger = logger;
        }

        public void EnqueueEvent(Func<Task> gameEvent)
        {
            _logger.LogInformation("Event enqueued at {Time}", DateTime.UtcNow);
            _gameEvents.Enqueue(gameEvent);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("GameFlowService started at {Time}", DateTime.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_gameEvents.TryDequeue(out var gameEvent))
                {
                    _logger.LogInformation("Processing an event at {Time}", DateTime.UtcNow);
                    await gameEvent();
                }

                await Task.Delay(100, stoppingToken);
            }

            _logger.LogInformation("GameFlowService stopped at {Time}", DateTime.UtcNow);
        }
    }
}
