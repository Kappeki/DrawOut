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
        private readonly IRoomRepo _roomRepo;
        public UserRepository(IRedisSettings settings, IRoomRepo roomRepo)
        {
            _redis = ConnectionMultiplexer.Connect(settings.ConnectionString);

            _database = _redis.GetDatabase();
            _roomRepo = roomRepo;
        }

        public async Task AddOrUpdateUserAsync(User user, TimeSpan? expiry = null)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            await _database.HashSetAsync(user._sessionKey,[
                new("Nickname", user.Nickname ?? string.Empty),
                new("Icon", user.Icon ?? string.Empty),
                new("MongoId", user.ObjectId ?? string.Empty)
            ]);

            if (expiry.HasValue)
            {
                await _database.KeyExpireAsync(user._sessionKey, expiry);
            }
        }

        //pomocna funkcija za dodavanje fielda u user hashu 
        public async Task AddToHashSet<T>(string setKey, Func<T, string> keySelector, T value, TimeSpan expiry)
        {
            string serializedObject = JsonConvert.SerializeObject(value);
            await _database.HashSetAsync(setKey, [new HashEntry(keySelector(value), serializedObject)]);
            await _database.KeyExpireAsync(setKey, expiry);
        }
        
        public async Task<T?> GetFromHashSet<T>(string setKey, string valueKey)
        {
            var value = await _database.HashGetAsync(setKey, valueKey);

            return value.IsNullOrEmpty ? default(T) : JsonConvert.DeserializeObject<T>(value);
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

            User user = new User();
            foreach (var entry in hashEntries)
            {
                string propName = entry.Name.ToString();
                PropertyInfo? propInfo = typeof(User).GetProperty(propName);

                if (propInfo != null && propInfo.CanWrite)
                {
                    object? propValue = JsonConvert.DeserializeObject(entry.Value!, propInfo.PropertyType);
                    propInfo.SetValue(user, propValue);
                }
            }

            user._id = ObjectId.Parse(GetFromHashSet<string>(key, "MongoId").Result);

            return user;
        }

        public async Task UpdateUserInRoomAsync(string roomId, User user, Dictionary<string, object> updates)
        {
            var roomObjectId = ObjectId.Parse(roomId);

            var filter = Builders<Room>.Filter.And(
                Builders<Room>.Filter.Eq("_id", roomObjectId),
                Builders<Room>.Filter.ElemMatch(r => r.Players, u => u._id == user._id)
            );

            var updateDefinitions = new List<UpdateDefinition<Room>>();
            foreach (var update in updates)
            {
                var updateDefinition = Builders<Room>.Update.Set($"Users.$.{update.Key}", update.Value);
                updateDefinitions.Add(updateDefinition);
            }
            var combinedUpdate = Builders<Room>.Update.Combine(updateDefinitions);
            await _roomRepo.UpdateRoomAsync(filter, combinedUpdate);
        }
    }
}
