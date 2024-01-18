using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Mappers;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Services.Contracts;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace DrawOutApp.Server.Services
{
    public class UserService : IUserService
    {

        private readonly IUserRepo _userRepo;
        private readonly IDistributedCache _cache;
        private readonly TimeSpan _cacheExpiryOptions;
       
        public UserService(IUserRepo userRepository, IDistributedCache cache) 
        {
            _userRepo = userRepository;
            _cache = cache; 
            _cacheExpiryOptions = TimeSpan.FromDays(7);
        }

        public async Task<Result<UserModel,string>> CreateUserAsync(UserModel userModel)
        {
            try
            {
                var newUser = UserMapper.ToEntity(userModel);
                if (string.IsNullOrEmpty(userModel.Nickname))
                {
                    newUser.Nickname = "Guest";
                    //newUser.Icon = "user.png";
                }
                await _userRepo.AddUserAsync(newUser);

                var cacheEntryOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheExpiryOptions
                };
                var newUserJson = JsonConvert.SerializeObject(newUser);
                await _cache.SetStringAsync(newUser.SessionId, newUserJson, cacheEntryOptions);

                return UserMapper.ToModel(newUser);
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error while creating user : {error}";
            }
        }
        public async Task<Result<UserModel?,string>> GetUserAsync(string sessionId)
        {
            UserModel userModel = default!;
            try
            {
                var userJson = await _cache.GetStringAsync(sessionId);
                if (!string.IsNullOrEmpty(userJson))
                {
                    return JsonConvert.DeserializeObject<UserModel>(userJson);
                }

                var user = await _userRepo.GetUserAsync(sessionId);

                if(user == null) return "User not found with Session ID: " + sessionId;
               
                userJson = JsonConvert.SerializeObject(user);
                await _cache.SetStringAsync(sessionId, userJson, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheExpiryOptions
                });

                userModel = UserMapper.ToModel(user);
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return "Failed to get user with error : " + error;
            }
            return userModel;
        }
        public async Task<Result<bool,string>> UpdateUserAsync(string sessionId, UserModel userModel)
        {
            try
            {
                var user = await _userRepo.GetUserAsync(sessionId);
                
                if (user == null)
                {
                    return "User not found with Session ID: " + sessionId;
                }

                if (userModel.Nickname != null && userModel.Icon != null)
                {
                    user.Nickname = userModel.Nickname;
                    user.Icon = userModel.Icon;
                }
                user = UserMapper.ToEntity(userModel);
                await _userRepo.UpdateUserAsync(sessionId, user);

                var userJson = JsonConvert.SerializeObject(UserMapper.ToModel(user));
                await _cache.SetStringAsync(sessionId, userJson, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheExpiryOptions
                });

                return true;
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Failed to update user with error : {error}";
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
