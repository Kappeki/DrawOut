using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Settings;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace DrawOutApp.Server.Repositories
{
    public class UserRepository : IUserRepo
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        public UserRepository(IRedisSettings settings)
        {
            _redis = ConnectionMultiplexer.Connect(settings.ConnectionString);
            _database = _redis.GetDatabase();
        }

        public async Task AddUserAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.SessionId = Guid.NewGuid().ToString();
            var userJson = JsonConvert.SerializeObject(user);
            await _database.StringSetAsync(user.SessionId, userJson);
        }

        public async Task DeleteUserAsync(string sessionId)
        {
            if (!_database.KeyExists(sessionId)) throw new ArgumentException($"User does not exist with ID {sessionId}");
            await _database.KeyDeleteAsync(sessionId);
        }

        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<User?> GetUserAsync(string sessionId)
        {
            var userJson = await _database.StringGetAsync(sessionId);
            if (!userJson.IsNullOrEmpty)
            {
                return JsonConvert.DeserializeObject<User>(userJson);
            }
            return null;
        }

        public async Task UpdateUserAsync(string sessionId, User user)
        {
            if (sessionId != user.SessionId) throw new ArgumentException("SessionId does not match");
            var userJson = JsonConvert.SerializeObject(user);
            await _database.StringSetAsync(sessionId, userJson);
        }
    }
}
