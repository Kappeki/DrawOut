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
        private readonly IMongoClient _mongoClient;

        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        public RoomRepository(IMongoDBSettings mongoSettings, IMongoClient mongoClient, IRedisSettings redisSettings)
        {
            var database = mongoClient.GetDatabase(mongoSettings.DatabaseName);
            _mongoClient = mongoClient;
            _roomsCollection = database.GetCollection<Room>(mongoSettings.RoomsCollectionName);

            _redis = ConnectionMultiplexer.Connect(redisSettings.ConnectionString);
            _database = _redis.GetDatabase();


            CreateIndexesAsync();
        }
        private async Task CreateIndexesAsync()
        {
            await _roomsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Room>(Builders<Room>.IndexKeys.Ascending(r => r.RoomName),
                new CreateIndexOptions { Unique = true }));
            
            //ovo da se brze hvata soba po url-u
            await _roomsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Room>(Builders<Room>.IndexKeys.Ascending(r => r.RoomURL), 
                new CreateIndexOptions { Unique = true}));
            
            await _roomsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Room>(Builders<Room>.IndexKeys.Ascending(r => r.PlayerCount))); 
            
            //da filtrira sobe koje nisu in-game
            await _roomsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Room>(Builders<Room>.IndexKeys.Ascending(r => r.GameState)));
            
            await _roomsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Room>(Builders<Room>.IndexKeys.Ascending(r => r.Password == null)));
            
            //ovaj index za myrooms tab da se otvori brze
            await _roomsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Room>(Builders<Room>.IndexKeys.Ascending(r => r.RoomAdminId)));
            //moze da se doda index da expiruje soba to cemo kasnije da vidimo

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
            await UpdateRoomAsync(filter, Builders<Room>.Update.Inc(r => r.PlayerCount, -1));
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

        public virtual async Task InsertIntoListAsync<TItem>(
        Expression<Func<Room, bool>> filter,
        Expression<Func<Room, IEnumerable<TItem>>> listProperty,
        TItem item, IClientSessionHandle? sesh = null)
        {
            var updateDefinition = Builders<Room>.Update.Push(listProperty, item);
            await UpdateRoomAsync(filter, updateDefinition, sesh);
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

        //conditional remove from list
        public virtual async Task RemoveFromListAsync<TItem>(
        FilterDefinition<Room> filter,
        Expression<Func<Room, IEnumerable<TItem>>> listProperty,
        Expression<Func<TItem, bool>> condition, 
        IClientSessionHandle? sesh = null) where TItem : class
        {
            var update = Builders<Room>.Update.PullFilter(listProperty, Builders<TItem>.Filter.Where(condition));
            await UpdateRoomAsync(filter, update, sesh);
        }
        
        //normal remove from list
        public virtual async Task RemoveFromListAsync<TItem>(Expression<Func<Room, bool>> filter,
        Expression<Func<Room, IEnumerable<TItem>>> listProperty, TItem value, 
        IClientSessionHandle? sesh = null)
        {
            var update = Builders<Room>.Update.Pull(listProperty, value);
            await UpdateRoomAsync(filter, update, sesh);
        }



    }
}
