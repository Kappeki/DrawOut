using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Mappers
{
    public class RoomMapper
    {
        public static RoomModel ToModel(Room entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            return new RoomModel
            {
                Id = entity.Id,
                RoomId = entity.RoomId,
                RoomName = entity.RoomName,
                Password = entity.Password,
                PlayerCount = entity.PlayerCount,
                RoomAdmin = UserMapper.ToModel(entity.RoomAdmin),
                Players = entity.Players?.Select(UserMapper.ToModel).ToList(),
                CustomWords = entity.CustomWords,
                RoomChat = entity.RoomChat?.Select(rc=>rc.ToBusinessModel()).ToList(),
                SelectedWordPack = entity.SelectedWordPack,
                GameState = entity.GameState,
                TeamScores = entity.TeamScores,
                RoundTime = entity.RoundTime
            };
        }

        // Converts from RoomModel to Room entity
        public static Room ToEntity(RoomModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            return new Room
            {
                Id = model.Id,
                RoomId = model.RoomId,
                RoomName = model.RoomName,
                Password = model.Password,
                PlayerCount = model.PlayerCount,
                RoomAdmin = UserMapper.ToEntity(model.RoomAdmin),
                Players = model.Players?.Select(UserMapper.ToEntity).ToList(),
                CustomWords = model.CustomWords,
                RoomChat = model.RoomChat?.Select(rc=>new ChatMessage(rc)).ToList(),
                SelectedWordPack = model.SelectedWordPack,
                GameState = model.GameState,
                TeamScores = model.TeamScores,
                RoundTime = model.RoundTime
            };
        }
    }
}
