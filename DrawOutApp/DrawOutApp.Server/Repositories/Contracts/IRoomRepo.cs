using DrawOutApp.Server.Entities;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace DrawOutApp.Server.Repositories.Contracts
{
    public interface IRoomRepo
    {
        Task<List<string>> GetAllPackNamesAsync();
        Task<List<string>> GetWordsByPackNameAsync(string packName);
        Task<Room> CreateRoomAsync(Room room);
        Task<Room?> GetRoomAsync(string roomId);
        IClientSessionHandle GetSession();
        Task AddPlayerToSetAsync(string id, string sessionId);
        Task RemovePlayerFromSetAsync(string id, string sessionId);
        Task<List<string>> GetPlayerSetAsync(string id);
        Task<IEnumerable<Room>> GetAllRoomsAsync(FilterDefinition<Room>? filter = null, SortDefinition<Room>? sort = null);
        Task UpdateRoomAsync(FilterDefinition<Room> filter, 
            UpdateDefinition<Room> update, 
            IClientSessionHandle? sesh = null); 
        Task DeleteRoomAsync(string roomId);
        Task DeleteManyRoomsAsync(Expression<Func<Room, bool>> filter);
   
        Task<Room?> GetRoomByFilterAsync(Expression<Func<Room, bool>> filter,
            IClientSessionHandle? sesh = null);

        Task CreateTTLIndexAsync(string collectionName, string fieldName, int expireAfterSeconds);
        Task CreateIndexesAsync();
    }
}
