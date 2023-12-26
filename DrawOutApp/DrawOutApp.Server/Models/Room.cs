using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DrawOutApp.Server.Models
{
    [BsonIgnoreExtraElements]
    public class Room
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public int Id { get; set; }
        [BsonElement("roomId")]
        public string RoomId { get; set; } = null!;
        [BsonElement("roomName")]
        public string RoomName { get; set; } = null!;
        [BsonElement("password")]
        public string Password { get; set; } = String.Empty;
        [BsonElement("playerCount")]
        public int PlayerCount { get; set; }
        [BsonElement("roomAdmin")]
        public Player? RoomAdmin { get; set; }
        [BsonElement("players")]
        public List<Player>? Players { get; set; }
        [BsonElement("customWords")]
        public List<String>? CustomWords { get; set; }
    }
}
