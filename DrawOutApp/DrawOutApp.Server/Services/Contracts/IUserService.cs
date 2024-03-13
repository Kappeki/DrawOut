using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Services.Contracts
{
    public interface IUserService
    {
        //poziva se kad se klikne play 
        Task<Result<UserModel,string>> CreateUserSessionAsync(UserModel userModel);
        
        //se poziva kad se vrati ponovo na sajt
        Task<Result<UserModel?,string>> GetUserAsync(string sessionId);
        
        //poziva se kad se promeni nickname, iconUrl
        Task<Result<bool, string>> UpdateUserPrefsAsync(string sessionId, UserModel userModel);

        Task AddRole(string roomId, string sessionId, Role role);
        Task RemoveRole(string roomId, string sessionId, Role role);
        
        Task DeleteUserAsync(string sessionId);
    }
}
