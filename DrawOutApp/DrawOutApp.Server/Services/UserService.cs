using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Mappers;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Services.Contracts;

namespace DrawOutApp.Server.Services
{
    public class UserService : IUserService
    {

        private readonly IUserRepo _userRepo;
        public UserService(IUserRepo userRepository) 
        {
            _userRepo = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<UserModel> CreateUserAsync(UserModel userModel)
        {
            //namestiti logiku za mongodzibi
            var newUser = UserMapper.ToEntity(userModel);
            if(string.IsNullOrEmpty(userModel.Nickname))
            {
                newUser.Nickname = "Guest";
            }
            await _userRepo.AddUserAsync(newUser);
            return UserMapper.ToModel(newUser);
        }

        public async Task<UserModel?> GetUserAsync(string sessionId)
        {
            var user = await _userRepo.GetUserAsync(sessionId);
            return user != null ? UserMapper.ToModel(user) : null;
        }

        public async Task UpdateUserAsync(string sessionId, UserModel userModel)
        {
            var user = await _userRepo.GetUserAsync(sessionId);
            if (user != null)
            {
                user = UserMapper.ToEntity(userModel);
                await _userRepo.UpdateUserAsync(sessionId, user);
            }
            else
            {
                throw new KeyNotFoundException($"User not found with Session ID: {sessionId}");
            }
        }

        public async Task DeleteUserAsync(string sessionId)
        {
            await _userRepo.DeleteUserAsync(sessionId);
        }

        public async Task AddRole(string sessionId, Role role)
        {
            var user = await _userRepo.GetUserAsync(sessionId);
            if (user != null)
            {
                user.Roles.Add(role);
                await _userRepo.UpdateUserAsync(sessionId, user);
            }
            else
            {
                throw new KeyNotFoundException($"User not found with Session ID: {sessionId}");
            }
        }

        public async Task RemoveRole(string sessionId, Role role)
        {
            var user = await _userRepo.GetUserAsync(sessionId);
            if (user != null)
            {
                user.Roles.Remove(role);
                await _userRepo.UpdateUserAsync(sessionId, user);
            }
            else
            {
                throw new KeyNotFoundException($"User not found with Session ID: {sessionId}");
            }
        }
    }
}
