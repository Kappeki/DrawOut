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

        public async Task<bool> AddGameAsync(Game game)
        {
            if (game == null) throw new ArgumentNullException(nameof(game));

            // Initialize a list to keep track of all the keys that we will be creating
            // This will be used for cleanup in case something fails
            var createdKeys = new List<RedisKey>();

            try
            {
                game.RoundIds = new List<string>();

                // Use a Redis transaction (it does not support rollback, but we can use it to execute all commands atomically)
                var tran = _database.CreateTransaction();

                for (int i = 1; i <= 8; i++)
                {
                    var round = new Round
                    {
                        GameSessionId = game.GameSessionId,
                        RoundNumber = i,
                    };

                    round.RoundId = $"round:{game.GameSessionId}:{i}";
                    game.RoundIds.Add(round.RoundId);

                    // Serialize round and add to transaction
                    var roundJson = JsonConvert.SerializeObject(round);
                    createdKeys.Add(round.RoundId);
                    tran.StringSetAsync(round.RoundId, roundJson);
                }

                var gameJson = JsonConvert.SerializeObject(game);
                createdKeys.Add(game.GameSessionId);
                tran.StringSetAsync(game.GameSessionId, gameJson);

                // Execute the transaction
                var committed = await tran.ExecuteAsync();

                if (!committed)
                {
                    // If the transaction was not committed successfully, attempt to clean up
                    await DeleteKeysAsync(createdKeys);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception here using your preferred logging framework
                await DeleteKeysAsync(createdKeys);
                throw; // Re-throw the exception to be handled further up the call stack
            }
        }

        public async Task<bool> UpdateGameAsync(string gameSessionId, Game game)
        {
            if(game == null || string.IsNullOrEmpty(gameSessionId)) return false;
            if (gameSessionId != game.GameSessionId) throw new ArgumentException("GameSessionId does not match");
       
            var gameJson = JsonConvert.SerializeObject(game);
            await _database.StringSetAsync(gameSessionId, gameJson);
            return true;
        }

        public async Task DeleteGameAsync(string gameSessionId)
        {
            if (!_database.KeyExists(gameSessionId)) { throw new ArgumentException("GameSessionId does not exist"); }
            await _database.KeyDeleteAsync(gameSessionId);
        }

        private async Task DeleteKeysAsync(IEnumerable<RedisKey> keys)
        {
            foreach (var key in keys)
            {
                await _database.KeyDeleteAsync(key);
            }
        }
    }
}
