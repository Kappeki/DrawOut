using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Mappers
{
    public static class UserMapper
    {
        // Converts from User entity to UserModel
        public static UserModel ToModel(User entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            return new UserModel
            {
                SeshKey = entity._sessionKey,
                MongoId = entity.ObjectId,
                Nickname = entity.Nickname,
                Roles = new HashSet<Role>(entity.Roles),
                Icon = entity.Icon,
                TeamId = entity.TeamId
            };
        }

        // Converts from UserModel to User entity
        public static User ToEntity(UserModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            return new User
            {
                _sessionKey = model.SeshKey,
                Nickname = model.Nickname,
                Roles = new HashSet<Role>(model.Roles),
                Icon = model.Icon,
                TeamId = model.TeamId
            };
        }
    }
}
