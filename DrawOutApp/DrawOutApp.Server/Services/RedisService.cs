
using DrawOutApp.Server.DataLayer;
using StackExchange.Redis;

namespace DrawOutApp.Server.Services
{
    public class RedisService : IRedisService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        public RedisService(IRedisSettings settings) {
            _redis = ConnectionMultiplexer.Connect(settings.ConnectionString);
            _database = _redis.GetDatabase();
        }

        public Task AddCustomWordsAsync(string roomId, IEnumerable<string> words)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetAsync(string key)
        {
            return await _database.StringGetAsync(key);
        }

        public Task<IEnumerable<string>> GetCustomWordsAsync(string roomId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveKeyAsync(string key)
        {
            throw new NotImplementedException();
        }

        public async Task SetAsync(string key, string value)
        {
            await _database.SetAddAsync(key, value);
        }
    }
}
