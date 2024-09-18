using DrawOutApp.Server.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DrawOutApp.Server.Entities
{
    public enum RoomState { Waiting, InGame }
    public enum RoundTime { Short = 40, Medium = 60, Long = 80 }
    [BsonIgnoreExtraElements]
    public class Room
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonIgnore]
        public ObjectId _id { get; set; }
        public string RoomName { get; set; } = null!;
        public string? Password { get; set; }
        public string? RoomURL { get; set; }
        public int PlayerCount { get; set; }
        public string RoomAdminId { get; set; } = null!;
        [BsonIgnoreIfNull]
        public string? PlayersSetKey { get; set; }

        [BsonIgnoreIfNull]
        public List<string>? CustomWords { get; set; }

        [BsonElement("wordPack")]
        public string? SelectedWordPack { get; set; }

        [BsonElement("roomState")]
        [BsonRepresentation(BsonType.String)]
        public RoomState RoomState { get; set; }

        [BsonElement("roundTime")]
        public RoundTime RoundTime { get; set; } //selektuje room admin

        [BsonElement("timeElapsed")]
        public DateTime TimeElapsed { get; set; }

        [BsonElement("expirationTime")]
        [BsonIgnoreIfNull]
        public DateTime? ExpirationTime { get; set; }

        [BsonIgnore]
        public string ObjectId { get { return _id.ToString(); } }

        public Room()
        {
            _id = MongoDB.Bson.ObjectId.GenerateNewId();
            PlayersSetKey = $"users-in-room:{ObjectId}";
            RoomState = RoomState.Waiting;
            RoundTime = RoundTime.Medium;
            TimeElapsed = DateTime.UtcNow;
        }
    }
}
