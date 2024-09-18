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
                // Create unique index on RoomName
                new CreateIndexModel<Room>(
                Builders<Room>.IndexKeys.Ascending(r => r.RoomName),
                new CreateIndexOptions { Unique = true }),

                // Create unique index on RoomURL for faster search
                new CreateIndexModel<Room>(
                Builders<Room>.IndexKeys.Ascending(r => r.RoomURL),
                new CreateIndexOptions { Unique = true }),

                // Index on PlayerCount for filtering rooms based on number of players
                new CreateIndexModel<Room>(
                Builders<Room>.IndexKeys.Ascending(r => r.PlayerCount)),

                // Index on RoomState for faster filtering of rooms that are not in-game
                new CreateIndexModel<Room>(
                Builders<Room>.IndexKeys.Ascending(r => r.RoomState)),

                // Index on rooms with no password for expiration logic
                //new CreateIndexModel<Room>(
                //Builders<Room>.IndexKeys.Ascending(r => string.IsNullOrEmpty(r.Password))),

                // Index on RoomAdminId for faster retrieval in "my rooms" section
                new CreateIndexModel<Room>(
                Builders<Room>.IndexKeys.Ascending(r => r.RoomAdminId)),

                // Create the TTL index for room expiration (initially not applied)
                new CreateIndexModel<Room>(
                Builders<Room>.IndexKeys.Ascending(r => r.ExpirationTime),
                new CreateIndexOptions { ExpireAfter = TimeSpan.FromSeconds(0) })
            };

            // Create all indexes in a single batch
            await _roomsCollection.Indexes.CreateManyAsync(indexModels);
        }


        public async Task<List<string>> GetAllPackNamesAsync()
        {
            var packs = await _wordPacksCollection.Find(Builders<WordPack>.Filter.Empty)
                                                 .Project(p => p.Name)
                                                 .ToListAsync();
            return packs;
        }

        // Method to retrieve words by pack name
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
            //compound assignment bas kul 
            filter ??= Builders<Room>.Filter.Empty;
            return await _roomsCollection.Find(filter).Sort(sort).ToListAsync();
        }

        public virtual async Task UpdateRoomAsync(Expression<Func<Room,bool>> filter, 
            UpdateDefinition<Room> update, 
            IClientSessionHandle? sesh = null)
        {
            if (sesh == null)
                await _roomsCollection.UpdateOneAsync(filter, update);
            else
                await _roomsCollection.UpdateOneAsync(sesh, filter, update);
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
