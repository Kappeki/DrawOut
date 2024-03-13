using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;
using System.Text.RegularExpressions;

namespace DrawOutApp.Server.Mappers
{
    public class RoomMapper
    {
        public static RoomModel ToModel(Room entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            return new RoomModel
            {
                RoomName = entity.RoomName,
                PasswordHash = entity.Password,
                RoomURL = entity.RoomURL,
                PlayerCount = entity.PlayerCount,
                RoomAdmin = UserMapper.ToModel(entity.RoomAdmin!),
                Players = entity.Players?.Select(UserMapper.ToModel).ToList(),
                CustomWords = entity.CustomWords,
                RoomChat = entity.RoomChat?.Select(rc=>rc.ToBusinessModel()).ToList(),
                SelectedWordPack = entity.SelectedWordPack,
                GameState = entity.GameState,
                RoundTime = entity.RoundTime
            };
        }

        // Converts from RoomModel to Room entity
        public static Room ToEntity(RoomModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            return new Room
            {
                RoomName = model.RoomName,
                RoomURL = model.RoomURL,
                PlayerCount = model.PlayerCount,
                RoomAdmin = UserMapper.ToEntity(model.RoomAdmin!),
                Players = model.Players?.Select(UserMapper.ToEntity).ToList(),
                CustomWords = model.CustomWords,
                RoomChat = model.RoomChat?.Select(rc=>new ChatMessage(rc)).ToList(),
                SelectedWordPack = model.SelectedWordPack,
                GameState = model.GameState,
                RoundTime = model.RoundTime
            };
        }

        public static string GenerateRoomURL(string roomName)
        {
            var sanitizedRoomName = Regex.Replace(roomName.ToLower(), @"[^a-z0-9]", "-");
            var uniquePart = Guid.NewGuid().ToString().Substring(0, 8); // Use part of a GUID for uniqueness
            return $"{sanitizedRoomName}-{uniquePart}";
        }
    }
}
