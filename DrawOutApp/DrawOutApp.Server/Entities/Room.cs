using DrawOutApp.Server.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace DrawOutApp.Server.Entities
{
    [BsonIgnoreExtraElements]
    public class Room
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("roomId")]
        public string RoomId { get; set; } = null!;

        [BsonElement("roomName")]
        public string RoomName { get; set; } = null!;

        [BsonElement("password")]
        public string Password { get; set; } = String.Empty;

        [BsonElement("roomURL")]
        public string RoomURL { get; set; }

        [BsonElement("playerCount")]
        public int PlayerCount { get; set; }

        [BsonElement("roomAdmin")]
        public User? RoomAdmin { get; set; }

        [BsonElement("players")]
        [JsonPropertyName("players")]
        public List<User>? Players { get; set; }

        [BsonElement("customWords")]
        [JsonPropertyName("customWords")]
        public List<string>? CustomWords { get; set; }

        [BsonElement("chatMessages")]
        [JsonPropertyName("chatMessages")]
        public List<ChatMessage>? RoomChat { get; set; }

        [BsonElement("wordPackId")]
        public ObjectId SelectedWordPack { get; set; }

        [BsonElement("gameState")]
        public GameState GameState { get; set; }

        public Dictionary<string, int> TeamScores { get; set; }

        [BsonElement("roundTime")]
        public RoundTime RoundTime { get; set; } //selektuje room admin

        [BsonElement("timeElapsed")]
        public DateTime TimeElapsed { get; set; }
        

        public Room()
        {
            Players = new List<User>();
            RoomChat = new List<ChatMessage>();
            TeamScores = new Dictionary<string, int>();
            GameState = GameState.Waiting;
            RoundTime = RoundTime.Medium;
            TimeElapsed = DateTime.UtcNow;
        }
    }
}
