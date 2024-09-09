using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Services.Contracts
{
    public interface IUserService
    {
        //poziva se kad se klikne play 
        Task<Result<UserModel,string>> CreateUserSessionAsync(UserPreferences userModel);
        
        //se poziva kad se vrati ponovo na sajt
        Task<Result<UserModel?,string>> GetUserAsync(string sessionId);
        Task<Result<UserModel?, string>> GetUserSessionAsync(HttpRequest request);
        
        //poziva se kad se promeni nickname, iconUrl
        Task<Result<bool, string>> UpdateUserPrefsAsync(string sessionId, UserPreferences userModel);

        Task AddRolesAsync(string sessionId, IEnumerable<Role> roles);
        Task RemoveRolesAsync(string sessionId, IEnumerable<Role> roles);
        Task DeleteUserAsync(string sessionId);
        Task SetConnectionIdAsync(string sessionId, string connectionId, TimeSpan? expiry = null);
        Task<string?> GetConnectionIdAsync(string sessionKey);
    }
}
