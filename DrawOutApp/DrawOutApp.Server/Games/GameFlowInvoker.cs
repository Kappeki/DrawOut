using DrawOutApp.Server.Games.Contracts;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace DrawOutApp.Server.Games
{
    public class GameFlowInvoker : BackgroundService
    {

        private readonly ILogger<GameFlowInvoker> _logger;
        private readonly ConcurrentDictionary<string, Channel<IGameCommand>> _gameChannels = new ConcurrentDictionary<string, Channel<IGameCommand>>();

        public GameFlowInvoker(ILogger<GameFlowInvoker> logger)
        {
            _logger = logger;
        }

        public async Task EnqueueCommandAsync(string gameId, IGameCommand command)
        {
            var channel = _gameChannels.GetOrAdd(gameId, _ => Channel.CreateUnbounded<IGameCommand>());
            await channel.Writer.WriteAsync(command);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("GameFlowInvoker started at {Time}", DateTime.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var gameId in _gameChannels.Keys)
                {
                    _ = ProcessGameQueueAsync(gameId, stoppingToken);
                }

                await Task.Delay(1000, stoppingToken); 
            }

            _logger.LogInformation("GameFlowInvoker stopped at {Time}", DateTime.UtcNow);
        }

        private async Task ProcessGameQueueAsync(string gameId, CancellationToken stoppingToken)
        {
            if (_gameChannels.TryGetValue(gameId, out var channel))
            {
                await foreach (var command in channel.Reader.ReadAllAsync(stoppingToken))
                {
                    try
                    {
                        _logger.LogInformation("Executing command {Command} for game {GameId} at {Time}", command.GetType().Name, gameId, DateTime.UtcNow);
                        await command.ExecuteAsync();

                        if (command is EndGameCommand)
                        {
                            _logger.LogInformation($"EndGameCommand processed, removing game {gameId} from queue.");
                            _gameChannels.TryRemove(gameId, out _);
                            break; 
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing command for game {GameId}", gameId);
                    }
                }
            }
        }


        /// <summary>
        ///  previous deprecated attempts
        /// </summary>

        /*private readonly ConcurrentDictionary<string,
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
                    _ = ProcessGameQueueAsync(gameId, stoppingToken);
                }
                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation("GameFlowInvoker stopped at {Time}", DateTime.UtcNow);
        }

        private async Task ProcessGameQueueAsync(string gameId, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Started processing queue for game {gameId} at {DateTime.UtcNow}");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_gameQueues.TryGetValue(gameId, out var queue))
                {
                    if (queue.TryDequeue(out var command))
                    {
                        try
                        {
                            _logger.LogInformation("Executing command for game {GameId} at {Time}", gameId, DateTime.UtcNow);
                            await command.ExecuteAsync();

                            // Check if this is the EndGameCommand and clean up
                            if (command is EndGameCommand)
                            {
                                _logger.LogInformation($"EndGameCommand processed, removing game {gameId} from queue.");
                                _gameQueues.TryRemove(gameId, out _);
                                break; // Stop processing this game as it has ended.
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error executing command for game {GameId}", gameId);
                        }
                    }
                }

                await Task.Delay(100, stoppingToken);  // Small delay to avoid excessive CPU usage
            }

            _logger.LogInformation($"Stopped processing queue for game {gameId} at {DateTime.UtcNow}");
        }*/


        /*protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("GameFlowInvoker started at {Time}", DateTime.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                var tasks = new List<Task>();

                foreach (var gameId in _gameQueues.Keys)
                {
                    if (_gameQueues.TryGetValue(gameId, out var queue) && queue.TryDequeue(out var command))
                    {
                        tasks.Add(Task.Run(async () =>
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
                        }, stoppingToken));
                    }
                }

                await Task.WhenAll(tasks); // Run tasks concurrently

                await Task.Delay(100, stoppingToken);
            }

            _logger.LogInformation("GameFlowInvoker stopped at {Time}", DateTime.UtcNow);
        }


        private readonly ConcurrentQueue<Func<Task>> _gameEvents = new ConcurrentQueue<Func<Task>>();

        private readonly ILogger<GameFlowInvoker> _logger;

        public GameFlowInvoker(ILogger<GameFlowInvoker> logger)
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
            _logger.LogInformation("GameFlowInvoker started at {Time}", DateTime.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_gameEvents.TryDequeue(out var gameEvent))
                {
                    _logger.LogInformation("Processing an event at {Time}", DateTime.UtcNow);
                    await gameEvent();
                }

                await Task.Delay(100, stoppingToken);
            }

            _logger.LogInformation("GameFlowInvoker stopped at {Time}", DateTime.UtcNow);
        }*/
    }
}
