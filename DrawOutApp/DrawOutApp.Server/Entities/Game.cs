using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DrawOutApp.Server.Entities
{
    public class Game
    {
        public string _cacheKey { get; set; } = null!;
        public string? RoomId { get; set; }
        public string? RedTeamId { get; set; }
        public string? BlueTeamId { get; set; }
        public int TotalRounds { get; set; }
        public List<string>? RoundIds { get; set; }
        public int CurrentRoundIndex { get; set; }
  
        public Game()
        {
            _cacheKey = $"game:{Guid.NewGuid()}";
            RoundIds = new List<string>();
        }
    }
    public class GameHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonIgnore]
        public ObjectId _id { get; set; }
        public List<string>? BlueTeam { get; set; }
        public List<string>? RedTeam { get; set; }
        public Dictionary<string,int>? Scoreboard { get; set; }
        public long ElapsedTime { get; set; }
        public string? Winner { get; set; }

        public GameHistory()
        {
            BlueTeam = new List<string>();
            RedTeam = new List<string>();
            Scoreboard = new Dictionary<string, int>()
            {
                {"Red", 0},
                {"Blue", 0}
            };
        }
    }
}
