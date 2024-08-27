using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DrawOutApp.Server.Design;

namespace DrawOutApp.Server.Entities
{
    public class Game
    {
        public string _id { get; set; } = null!;
        public string RoomId { get; set; } = null!;
        public int BlueTeamScore { get; set; }
        public int RedTeamScore { get; set; }
        public int TotalRounds { get; set; }
        public int CurrentRound { get; set; }
        public string? CurrentPainter { get; set; }
        public string? SelectedWord { get; set; }
        public Dictionary<string,string>? TeamLeaders { get; set; }
        
        public IGameState State { get; private set; }
        public void SetState(IGameState state)
        {
            State = state;
        }
        public string SerializeState()
        {
            return State._name;
        }

        public void DeserializeState(string stateName)
        {
            switch (stateName)
            {
                case "WaitingForPlayers":
                    State = new WaitingForPlayers();
                    break;
                case "InProgress":
                    State = new InProgress();
                    break;
                case "Completed":
                    State = new Completed();
                    break;
                default: return;
            }
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
