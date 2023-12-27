using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DrawOutApp.Server.Models
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
        public Player? RoomAdmin { get; set; }

        [BsonElement("players")]
        public List<Player>? Players { get; set; }

        [BsonElement("customWords")]
        public List<String>? CustomWords { get; set; }

        [BsonElement("chatMessages")]
        public List<ChatMessage>? ChatMessages { get; set; }

        [BsonElement("wordPackId")]
        public ObjectId SelectedWordPack { get; set; }

        [BsonElement("gameState")]
        public GameState GameState { get; set; }

        public Dictionary<String, int> TeamScores { get; set; } //nadograditi

        public RoundTime RoundTime { get; set; } //selektuje room admin

        public Room()
        {
            Players = new List<Player>();
            ChatMessages = new List<ChatMessage>();
            TeamScores = new Dictionary<String, int>();
            GameState = GameState.Waiting;
            RoundTime = RoundTime.Medium;
        }

        public void ClearChat() 
        { 
            ChatMessages?.Clear(); 
        }

        public bool IsReady() //proverava da li prvo ima dovoljno igraca, a onda da li su timovi izjednaceni, tj da li je svako izabrao tim
        {
            throw new NotImplementedException();
        }
        
    }
}
