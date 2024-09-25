using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Settings;
using MongoDB.Bson;
using MongoDB.Driver;
using StackExchange.Redis;
using System.Linq.Expressions;

namespace DrawOutApp.Server.Repositories
{
    public class RoomRepository : IRoomRepo
    {
        private readonly IMongoCollection<Room> _roomsCollection;
        private readonly IMongoCollection<WordPack> _wordPacksCollection;
        private readonly IMongoClient _mongoClient;

        private readonly IDatabase _database;
        public RoomRepository(IMongoDBSettings mongoSettings, IMongoClient mongoClient, IConnectionMultiplexer redis)
        {
            var database = mongoClient.GetDatabase(mongoSettings.DatabaseName);
            _mongoClient = mongoClient;
            _roomsCollection = database.GetCollection<Room>(mongoSettings.RoomsCollectionName);
            _wordPacksCollection = database.GetCollection<WordPack>(mongoSettings.WordPacksCollectionName);

            _database = redis.GetDatabase();

        }
        public async Task CreateIndexesAsync()
        {
            var indexModels = new List<CreateIndexModel<Room>>
            {
                new CreateIndexModel<Room>(
                Builders<Room>.IndexKeys.Ascending(r => r.RoomName),
                new CreateIndexOptions { Unique = true }),

                new CreateIndexModel<Room>(
                Builders<Room>.IndexKeys.Ascending(r => r.RoomURL),
                new CreateIndexOptions { Unique = true }),

                new CreateIndexModel<Room>(
                Builders<Room>.IndexKeys.Ascending(r => r.PlayerCount)),

                
                new CreateIndexModel<Room>(
                Builders<Room>.IndexKeys.Ascending(r => r.RoomState)),

                new CreateIndexModel<Room>(
                Builders<Room>.IndexKeys.Ascending(r => r.RoomAdminId)),

                new CreateIndexModel<Room>(
                Builders<Room>.IndexKeys.Ascending(r => r.ExpirationTime),
                new CreateIndexOptions { ExpireAfter = TimeSpan.FromSeconds(0) })
            };

            await _roomsCollection.Indexes.CreateManyAsync(indexModels);
        }

        public async Task<List<string>> GetAllPackNamesAsync()
        {
            var packs = await _wordPacksCollection.Find(Builders<WordPack>.Filter.Empty)
                                                 .Project(p => p.Name)
                                                 .ToListAsync();
            return packs;
        }
        public async Task<List<string>> GetWordsByPackNameAsync(string packName)
        {
            var filter = Builders<WordPack>.Filter.Eq(p => p.Name, packName);
            var pack = await _wordPacksCollection.Find(filter).FirstOrDefaultAsync();
            return pack?.Words ?? new List<string>();
        }

        public IClientSessionHandle GetSession()
        {
            return _mongoClient.StartSession();
        }

        public async Task AddPlayerToSetAsync(string id, string sessionId)
        {
            var objId = ObjectId.Parse(id);
            var filter = Builders<Room>.Filter.Eq("_id", objId);
            await UpdateRoomAsync(filter, Builders<Room>.Update.Inc(r => r.PlayerCount, 1));
            await _database.SetAddAsync($"users-in-room:{id}", sessionId);
        }
        public async Task RemovePlayerFromSetAsync(string id, string sessionId)
        {
            var objId = ObjectId.Parse(id);
            var filter = Builders<Room>.Filter.Eq("_id", objId);
            var room = await _roomsCollection.Find(filter).FirstOrDefaultAsync();
            if (room != null && room.PlayerCount > 0)
            {
                await UpdateRoomAsync(filter, Builders<Room>.Update.Inc(r => r.PlayerCount, -1));
            }
            await _database.SetRemoveAsync($"users-in-room:{id}", sessionId);
        }
        public async Task<List<string>> GetPlayerSetAsync(string id)
        {
            return (await _database.SetMembersAsync($"users-in-room:{id}")).Select(x => x.ToString()).ToList();
        }
        public async Task<Room> CreateRoomAsync(Room room)
        {
            await _roomsCollection.InsertOneAsync(room);
            return room;
        }
        public async Task<Room?> GetRoomAsync(string id)
        {
            var objId = ObjectId.Parse(id);
            var filter = Builders<Room>.Filter.Eq("_id", objId);
            return await _roomsCollection.Find(filter).FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<Room>> GetAllRoomsAsync(FilterDefinition<Room>? filter = null, SortDefinition<Room>? sort = null)
        {
            filter ??= Builders<Room>.Filter.Empty;
            return await _roomsCollection.Find(filter).Sort(sort).ToListAsync();
        }

        public virtual async Task UpdateRoomAsync(FilterDefinition<Room> filter, 
            UpdateDefinition<Room> update, 
            IClientSessionHandle? sesh = null)
        {
            if (sesh == null)
                await _roomsCollection.UpdateOneAsync(filter, update);
            else
                await _roomsCollection.UpdateOneAsync(sesh, filter, update);
        }

        public async Task DeleteRoomAsync(string id)
        {
            var objId = ObjectId.Parse(id);
            var filter = Builders<Room>.Filter.Eq("_id", objId);
            await _roomsCollection.DeleteOneAsync(filter);
        }

        public async Task DeleteManyRoomsAsync(Expression<Func<Room, bool>> filter)
        {
            await _roomsCollection.DeleteManyAsync(filter);
        }

        public async Task<Room?> GetRoomByFilterAsync(Expression<Func<Room, bool>> filter, 
            IClientSessionHandle? sesh = null)
        {
            if (sesh == null)
            {
                return await _roomsCollection.Find(filter).FirstOrDefaultAsync();
            }
            else
            {
                return await _roomsCollection.Find(sesh, filter).FirstOrDefaultAsync();
            }
        }

        public async Task CreateTTLIndexAsync(string collectionName, string fieldName, int expireAfterSeconds)
        {
            var indexKeysDefinition = Builders<Room>.IndexKeys.Ascending(fieldName);
            var indexModel = new CreateIndexModel<Room>(indexKeysDefinition, new CreateIndexOptions
            {
                ExpireAfter = TimeSpan.FromSeconds(expireAfterSeconds)
            });

            await _roomsCollection.Indexes.CreateOneAsync(indexModel);
        }

    }
}
