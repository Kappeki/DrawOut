using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DrawOutApp.Server.Entities
{
    public enum GameState { WaitingForPlayers, Standby, InProgress, Steal, RoundEnded}
    public class Game
    {
        //static once game is created
        public string _id { get; set; } = null!;
        public string RoomId { get; set; } = null!;
        public List<string>? PainterOrder { get; set; }
        public Dictionary<string,string>? TeamLeaders { get; set; }
        public int TotalRounds { get; set; }
        
        //game round info
        public GameState GameState { get; set; }
        public int BlueScore { get; set; }
        public int RedScore { get; set; }
        public int CurrentRound { get; set; }
        public string? CurrentPainter { get; set; }
        public string? SelectedWord { get; set; }
        public int MainTimer { get; set; }
        public int StealTimer { get; set; }
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
