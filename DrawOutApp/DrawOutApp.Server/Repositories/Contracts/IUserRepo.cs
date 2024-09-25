using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Repositories.Contracts
{
    public interface IUserRepo
    {
        Task<User?> GetUserAsync(string sessionId);
        Task AddToHashSet<T>(string setKey, Func<T, string> keySelector, T value, TimeSpan? expiry = null);
        Task<T?> GetFromHashSet<T>(string setKey, string valueKey);
        Task AddOrUpdateUserAsync(User user, TimeSpan? expiry = null);
        Task DeleteUserAsync(string sessionId);
        Task<List<string>> GetAllNicknamesAsync();
        Task<List<string>> GetAllIconsAsync();
    }
}
