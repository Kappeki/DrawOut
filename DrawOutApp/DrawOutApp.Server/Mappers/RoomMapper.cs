using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;
using MongoDB.Bson;
using System.Text.RegularExpressions;

namespace DrawOutApp.Server.Mappers
{
    public class RoomMapper
    {
        public static RoomModel ToModel(Room entity, List<ChatMessage>? chat = null)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            return new RoomModel
            {
                RoomId = entity.ObjectId,
                CurrentGameId = entity.CurrentGameId,
                RoomName = entity.RoomName,
                PasswordHash = entity.Password,
                RoomURL = entity.RoomURL,
                PlayerCount = entity.PlayerCount,
                RoomAdmin = UserMapper.ToModel(entity.RoomAdmin!),
                Players = entity.Players?.Select(UserMapper.ToModel).ToList(),
                CustomWords = entity.CustomWords,
                RoomChat = chat,
                SelectedWordPack = entity.SelectedWordPack,
                GameState = entity.GameState,
                RoundTime = entity.RoundTime
            };
        }

        //potencijalni problem ako dodje do neceg sa idjem 
        public static Room ToEntity(RoomModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            return new Room
            {
                _id = ObjectId.Parse(model.RoomId),
                CurrentGameId = model.CurrentGameId,
                RoomName = model.RoomName,
                RoomURL = model.RoomURL,
                PlayerCount = model.PlayerCount,
                RoomAdmin = UserMapper.ToEntity(model.RoomAdmin!),
                Players = model.Players?.Select(UserMapper.ToEntity).ToList(),
                CustomWords = model.CustomWords,
                SelectedWordPack = model.SelectedWordPack,
                GameState = model.GameState,
                RoundTime = model.RoundTime
            };
        }
    }
}
