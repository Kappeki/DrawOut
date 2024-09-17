using DrawOutApp.Server.Games.Contracts;
using System.Collections.Concurrent;

namespace DrawOutApp.Server.Games
{
    public class GameFlowInvoker : BackgroundService
    {
        private readonly ConcurrentDictionary<string,
            ConcurrentQueue<IGameCommand>> _gameQueues = new();
        private readonly ILogger<GameFlowInvoker> _logger;

        public GameFlowInvoker(ILogger<GameFlowInvoker> logger)
        {
            _logger = logger;
        }

        public void EnqueueCommand(string gameId, IGameCommand command)
        {
            var queue = _gameQueues.GetOrAdd(gameId, new ConcurrentQueue<IGameCommand>());
            queue.Enqueue(command);
            _logger.LogInformation($"Command enqueued for game {gameId} at {DateTime.UtcNow}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("GameFlowInvoker started at {Time}", DateTime.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var gameId in _gameQueues.Keys)
                {
                    if (_gameQueues.TryGetValue(gameId, out var queue) && queue.TryDequeue(out var command))
                    {
                        try
                        {
                            _logger.LogInformation("Executing command for game {GameId} at {Time}", gameId, DateTime.UtcNow);
                            await command.ExecuteAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error executing command for game {GameId}", gameId);
                        }
                    }
                }

                await Task.Delay(100, stoppingToken);
            }

            _logger.LogInformation("GameFlowInvoker stopped at {Time}", DateTime.UtcNow);
        }


        //private readonly ConcurrentQueue<Func<Task>> _gameEvents = new ConcurrentQueue<Func<Task>>();

        //private readonly ILogger<GameFlowInvoker> _logger;

        //public GameFlowInvoker(ILogger<GameFlowInvoker> logger)
        //{
        //    _logger = logger;
        //}

        //public void EnqueueEvent(Func<Task> gameEvent)
        //{
        //    _logger.LogInformation("Event enqueued at {Time}", DateTime.UtcNow);
        //    _gameEvents.Enqueue(gameEvent);
        //}

        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    _logger.LogInformation("GameFlowInvoker started at {Time}", DateTime.UtcNow);

        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        if (_gameEvents.TryDequeue(out var gameEvent))
        //        {
        //            _logger.LogInformation("Processing an event at {Time}", DateTime.UtcNow);
        //            await gameEvent();
        //        }

        //        await Task.Delay(100, stoppingToken);
        //    }

        //    _logger.LogInformation("GameFlowInvoker stopped at {Time}", DateTime.UtcNow);
        //}
    }
}
