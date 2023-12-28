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
                SessionId = entity.SessionId,
                GameSessionId = entity.GameSessionId,
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
                SessionId = model.SessionId,
                GameSessionId = model.GameSessionId,
                Nickname = model.Nickname,
                Roles = new HashSet<Role>(model.Roles),
                Icon = model.Icon,
                TeamId = model.TeamId
            };
        }
    }
}
