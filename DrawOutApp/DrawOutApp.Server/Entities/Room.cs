using DrawOutApp.Server.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace DrawOutApp.Server.Entities
{
    public enum GameState { Waiting, InGame }
    public enum RoundTime { Short = 40, Medium = 60, Long = 80 }

    [BsonIgnoreExtraElements]
    public class Room
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public int Id { get; set; }

        [BsonElement("roomId")]
        public String RoomId { get; set; } = null!;

        [BsonElement("roomName")]
        public String RoomName { get; set; } = null!;

        [BsonElement("password")]
        public String Password { get; set; } = String.Empty;

        [BsonElement("playerCount")]
        public int PlayerCount { get; set; }

        [BsonElement("roomAdmin")]
        public User? RoomAdmin { get; set; }

        [BsonElement("players")]
        [JsonPropertyName("players")]
        public List<User>? Players { get; set; }

        [BsonElement("customWords")]
        [JsonPropertyName("customWords")]
        public List<String>? CustomWords { get; set; }

        [BsonElement("chatMessages")]
        [JsonPropertyName("chatMessages")]
        public List<ChatMessage>? RoomChat { get; set; }

        [BsonElement("wordPackId")]
        public ObjectId SelectedWordPack { get; set; }

        [BsonElement("gameState")]
        public GameState GameState { get; set; }

        public Dictionary<String, int> TeamScores { get; set; } //nadograditi

        [BsonElement("roundTime")]
        public RoundTime RoundTime { get; set; } //selektuje room admin

        public Room()
        {
            Players = new List<User>();
            RoomChat = new List<ChatMessage>();
            TeamScores = new Dictionary<String, int>();
            GameState = GameState.Waiting;
            RoundTime = RoundTime.Medium;
        }
    }
}
