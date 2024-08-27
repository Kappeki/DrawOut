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

        public async Task<Game?> GetGameAsync(string gameId)
        {
            var hashEntries = await _database.HashGetAllAsync(gameId);
            throw new NotImplementedException();
        }
        public async Task<bool> SaveGameAsync(Game game, TimeSpan? expiry)
        {
            if (game == null) throw new ArgumentNullException(nameof(game));
            game._id = $"game:{game.RoomId}";

            //NOT IMPLEMENTED//

            if (expiry.HasValue)
            {
                await _database.KeyExpireAsync(game._id, expiry);
            }
            return true;
        }

        public async Task DeleteGameAsync(string gameId)
        {
            if (!_database.KeyExists(gameId)) { throw new ArgumentException("GameSessionId does not exist"); }
            await _database.KeyDeleteAsync(gameId);
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
