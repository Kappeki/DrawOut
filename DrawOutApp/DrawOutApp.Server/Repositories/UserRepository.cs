using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Settings;
using StackExchange.Redis;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Reflection;

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

        public async Task AddOrUpdateUserAsync(User user, TimeSpan? expiry = null)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var rolesSerialized = user.SerializeRoles();

            await _database.HashSetAsync(user._sessionKey,[
                new("Nickname", user.Nickname ?? string.Empty),
                new("Icon", user.Icon ?? string.Empty),
                new("Roles", rolesSerialized ?? string.Empty)
            ]);

            if (expiry.HasValue)
            {
                await _database.KeyExpireAsync(user._sessionKey, expiry);
            }
        }

        //pomocna funkcija za dodavanje fielda u user hashu 
        public async Task AddToHashSet<T>(string setKey, Func<T, string> keySelector, T value, TimeSpan? expiry = null)
        {
            string serializedObject;

            if (value is string stringValue)
            {
                serializedObject = stringValue;
            }
            else
            {
                serializedObject = JsonConvert.SerializeObject(value);
            }

            await _database.HashSetAsync(setKey, [new HashEntry(keySelector(value), serializedObject)]);

            if (expiry.HasValue)
            {
                await _database.KeyExpireAsync(setKey, expiry);
            }
        }

        public async Task<T?> GetFromHashSet<T>(string setKey, string valueKey)
        {
            var value = await _database.HashGetAsync(setKey, valueKey);
            if(typeof(T) == typeof(string))
            {
                return value.IsNullOrEmpty ? default(T) : (T)(object)value.ToString();
            }
            return value.IsNullOrEmpty ? default(T) : JsonConvert.DeserializeObject<T>(value!);
        }

        public async Task<string> GetConnIdFromHash(string setKey, string valueKey)
        {
            var value = await _database.HashGetAsync(setKey, valueKey);
            return value.IsNullOrEmpty ? string.Empty : value.ToString();
        }

        public async Task DeleteUserAsync(string sessionKey)
        {
            if (!_database.KeyExists(sessionKey)) throw new ArgumentException($"User does not exist with ID {sessionKey}");
            await _database.KeyDeleteAsync(sessionKey);
        }

        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<User?> GetUserAsync(string key)
        {
            var hashEntries = await _database.HashGetAllAsync(key);
            if (hashEntries.Length == 0)
            {
                return null;
            }

            User user = new User()
            {
                _sessionKey = key,
                Nickname = hashEntries.FirstOrDefault(x => x.Name == "Nickname").Value,
                Icon = hashEntries.FirstOrDefault(x => x.Name == "Icon").Value,
            };

            string rolesSerialized = hashEntries.FirstOrDefault(x => x.Name == "Roles").Value!;
            user.DeserializeRoles(rolesSerialized);
            return user;
        }

    }
}
