using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Settings;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace DrawOutApp.Server.Repositories
{
    public class GameRepository : IGameRepo
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        public GameRepository(IRedisSettings settings)
        {
            _redis = ConnectionMultiplexer.Connect(settings.ConnectionString);
            _database = _redis.GetDatabase();
        }
        public async Task<Game?> GetGameAsync(string gameSessionId)
        {
            if (!_database.KeyExists(gameSessionId)) throw new ArgumentException($"Game does not exist with ID {gameSessionId}");
            var gameJson = await _database.StringGetAsync(gameSessionId);
            if (!gameJson.IsNullOrEmpty)
            {
                return JsonConvert.DeserializeObject<Game>(gameJson);
            }
            return null;
        }

        public async Task AddGameAsync(Game game)
        {
            if (game == null) throw new ArgumentNullException(nameof(game));
            var gameJson = JsonConvert.SerializeObject(game);
            await _database.StringSetAsync(game.GameSessionId, gameJson);
        }

        public async Task UpdateGameAsync(string gameSessionId, Game game)
        {
            if(game == null) throw new ArgumentNullException(nameof(game));
            if (gameSessionId != game.GameSessionId) throw new ArgumentException("GameSessionId does not match");
            var gameJson = JsonConvert.SerializeObject(game);
            await _database.StringSetAsync(gameSessionId, gameJson);
        }

        public async Task DeleteGameAsync(string gameSessionId)
        {
            if (!_database.KeyExists(gameSessionId)) { throw new ArgumentException("GameSessionId does not exist"); }
            await _database.KeyDeleteAsync(gameSessionId);
        }
    }
}
