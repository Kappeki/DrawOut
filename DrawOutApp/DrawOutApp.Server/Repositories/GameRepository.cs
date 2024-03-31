using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Settings;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Reflection;

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

        public ITransaction BeginTransaction()
        {
            return _database.CreateTransaction();
        }

        public async Task<Game?> GetGameAsync(string gameSessionId)
        {
            var hashEntries = await _database.HashGetAllAsync(gameSessionId);
            if (hashEntries.Length == 0) return null;
            Game game = new Game();
            foreach (var entry in hashEntries)
            {
                string propName = entry.Name.ToString();
                PropertyInfo? propInfo = typeof(Game).GetProperty(propName);

                if (propInfo != null && propInfo.CanWrite)
                {
                    if (propName == "RoundsListKey")
                    { 
                        var roundIds = await _database.ListRangeAsync(entry.Value.ToString());
                        game.RoundIds = roundIds.Select(id => id.ToString()).ToList();
                    }
                    else
                    {
                        object? propValue = JsonConvert.DeserializeObject(entry.Value!, propInfo.PropertyType);
                        propInfo.SetValue(game, propValue);
                    }
                }
            }
            return game;
        }

        public async Task AddGameAsync(Game game, TimeSpan? expiry = null)
        {
            if(game == null) throw new ArgumentNullException(nameof(game));

            string roundsListKey = $"{game._cacheKey}:rounds";

            if (game.RoundIds != null && game.RoundIds.Any())
            {
                await _database.ListRightPushAsync(roundsListKey,
                    game.RoundIds.Select(id => (RedisValue)id).ToArray());
            }

            await _database.HashSetAsync(game._cacheKey, [
                new("RoomId", game.RoomId ?? string.Empty),
                new("RedTeamId", game.RedTeamId ?? string.Empty),
                new("BlueTeamId", game.BlueTeamId ?? string.Empty),
                new("TotalRounds", game.TotalRounds.ToString() ?? string.Empty),
                new("CurrentRoundIndex", game.CurrentRoundIndex.ToString() ?? string.Empty),
                new("RoundsListKey", roundsListKey)
            ]);
           
            if (expiry.HasValue)
            {
                await _database.KeyExpireAsync(game._cacheKey, expiry);
                await _database.KeyExpireAsync(roundsListKey, expiry);
            }

            //_database.IncrementHashField(game._cacheKey, "CurrentRoundIndex", 1);
        }

        public async Task AddGameAsync(Game game, ITransaction tran, TimeSpan? expiry = null)
        {
            if (game == null) throw new ArgumentNullException(nameof(game));

            string roundsListKey = $"{game._cacheKey}:rounds";

            if (game.RoundIds != null && game.RoundIds.Any())
            {
                await tran.ListRightPushAsync(roundsListKey,
                    game.RoundIds.Select(id => (RedisValue)id).ToArray());
            }

            await tran.HashSetAsync(game._cacheKey, [
                new("RoomId", game.RoomId ?? string.Empty),
                new("RedTeamId", game.RedTeamId ?? string.Empty),
                new("BlueTeamId", game.BlueTeamId ?? string.Empty),
                new("TotalRounds", game.TotalRounds.ToString() ?? string.Empty),
                new("CurrentRoundIndex", game.CurrentRoundIndex.ToString() ?? string.Empty),
                new("RoundsListKey", roundsListKey)
            ]);

            if (expiry.HasValue)
            {
                await tran.KeyExpireAsync(game._cacheKey, expiry);
                await tran.KeyExpireAsync(roundsListKey, expiry);
            }

            //_database.IncrementHashField(game._cacheKey, "CurrentRoundIndex", 1);
        }


        /*public async Task<bool> UpdateGameAsync(string gameSessionId, Game game)
        {
            if(game == null || string.IsNullOrEmpty(gameSessionId)) return false;
            if (gameSessionId != game._cacheKey) throw new ArgumentException("GameSessionId does not match");
       
            var gameJson = JsonConvert.SerializeObject(game);
            await _database.StringSetAsync(gameSessionId, gameJson);
            return true;
        }*/

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
        //pomocna funkcija za izvlacenje podataka iz hash seta
        public async Task<T?> GetFromHashSet<T>(string setKey, string valueKey)
        {
            var value = await _database.HashGetAsync(setKey, valueKey);

            return value.IsNullOrEmpty ? default : JsonConvert.DeserializeObject<T>(value);
        }

        //u servis logika za startovanje igre, ukljucuje kreiranje rundi

    }
}
