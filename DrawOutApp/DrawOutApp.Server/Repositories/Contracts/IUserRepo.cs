using DrawOutApp.Server.Entities;

namespace DrawOutApp.Server.Repositories.Contracts
{
    public interface IUserRepo
    {
        Task<User?> GetUserAsync(string sessionId);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task AddUserAsync(User user);
        Task UpdateUserAsync(string sessionId, User user);
        Task DeleteUserAsync(string sessionId);
    }
}
