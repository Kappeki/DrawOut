using DrawOutApp.Server.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DrawOutApp.Server.Entities
{
    [BsonIgnoreExtraElements]
    public class Room
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonIgnore]
        public ObjectId _id { get; set; }

        [BsonElement("roomName")]
        public string RoomName { get; set; } = null!;
        public string? Password { get; set; } = String.Empty;

        [BsonElement("roomURL")]
        public string? RoomURL { get; set; }

        [BsonElement("playerCount")]
        public int PlayerCount { get; set; }

        [BsonElement("roomAdmin")]
        public User? RoomAdmin { get; set; }

        [BsonElement("players")]
        [JsonPropertyName("players")]
        public List<User>? Players { get; set; }

        [BsonElement("gameHistory")]
        [BsonIgnoreIfNull]
        public List<GameHistory>? GameHistory { get; set; }

        [BsonElement("customWords")]
        [JsonPropertyName("customWords")]
        [BsonIgnoreIfNull]
        public List<string>? CustomWords { get; set; }

        [BsonElement("chatMessages")]
        [JsonPropertyName("chatMessages")]
        public List<ChatMessage>? RoomChat { get; set; }

        [BsonElement("wordPack")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? SelectedWordPack { get; set; }

        [BsonElement("gameState")]
        public GameState GameState { get; set; }

        [BsonElement("roundTime")]
        public RoundTime RoundTime { get; set; } //selektuje room admin

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonIgnore]
        public string ObjectId { get { return _id.ToString(); } }

        public Room()
        {
            _id = MongoDB.Bson.ObjectId.GenerateNewId();
            Players = new List<User>();
            RoomChat = new List<ChatMessage>();
            GameState = GameState.Waiting;
            RoundTime = RoundTime.Medium;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
