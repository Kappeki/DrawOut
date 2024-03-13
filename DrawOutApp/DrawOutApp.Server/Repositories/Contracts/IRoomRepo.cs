using DrawOutApp.Server.Entities;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace DrawOutApp.Server.Repositories.Contracts
{
    public interface IRoomRepo
    {
        Task<Room> CreateRoomAsync(Room room);
        Task<Room?> GetRoomAsync(string roomId);
        IClientSessionHandle GetSession();
        Task<IEnumerable<Room>> GetAllRoomsAsync(FilterDefinition<Room>? filter = null, SortDefinition<Room>? sort = null);
        Task UpdateRoomAsync(Expression<Func<Room, bool>> filter, UpdateDefinition<Room> update, IClientSessionHandle? sesh = null);
        Task UpdateRoomAsync(FilterDefinition<Room> filter, 
            UpdateDefinition<Room> update, 
            IClientSessionHandle? sesh = null); // Optional based on usage
        Task DeleteRoomAsync(string roomId);
        Task DeleteManyRoomsAsync(Expression<Func<Room, bool>> filter);

        // New methods based on the updated RoomRepository
        Task InsertIntoListAsync<TItem>(Expression<Func<Room, bool>> filter, 
            Expression<Func<Room, IEnumerable<TItem>>> listProperty, 
            TItem item, IClientSessionHandle? sesh = null);
        Task RemoveFromListAsync<TItem>(FilterDefinition<Room> filter, 
            Expression<Func<Room, IEnumerable<TItem>>> listProperty, 
            Expression<Func<TItem, bool>> condition, IClientSessionHandle? sesh = null) where TItem : class;
        Task RemoveFromListAsync<TItem>(Expression<Func<Room, bool>> filter, 
            Expression<Func<Room, IEnumerable<TItem>>> listProperty, 
            TItem value, IClientSessionHandle? sesh = null);
        Task<Room?> GetRoomByFilterAsync(Expression<Func<Room, bool>> filter,
            IClientSessionHandle? sesh = null);
    }
}
