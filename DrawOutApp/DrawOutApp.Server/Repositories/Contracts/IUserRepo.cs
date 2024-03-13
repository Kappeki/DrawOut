using DrawOutApp.Server.Entities;

namespace DrawOutApp.Server.Repositories.Contracts
{
    public interface IUserRepo
    {
        Task<User?> GetUserAsync(string sessionId);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task AddToHashSet<T>(string setKey, Func<T, string> keySelector, T value, TimeSpan expiry);
        Task AddOrUpdateUserAsync(User user, TimeSpan? expiry = null);
        Task UpdateUserInRoomAsync(string roomId, User user, Dictionary<string, object> updates);
        Task DeleteUserAsync(string sessionId);
    }
}
