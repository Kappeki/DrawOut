using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Security.Permissions;
using System.Text.Json.Serialization;

namespace DrawOutApp.Server.Models
{
    public enum GameState { Waiting, InGame }

    public enum RoundTime { Short = 40, Medium = 60, Long = 80 }

    public class RoomModel
    {
       
        public int Id { get; set; }
        public String RoomId { get; set; } = null!;
        public String RoomName { get; set; } = null!;
        public String Password { get; set; } = String.Empty;
        public int PlayerCount { get; set; }
        public UserModel? RoomAdmin { get; set; }
        public List<UserModel>? Players { get; set; }
        public List<String>? CustomWords { get; set; }
        public List<ChatMessageModel>? RoomChat { get; set; }
        public ObjectId SelectedWordPack { get; set; }
        public GameState GameState { get; set; }
        public Dictionary<String, int> TeamScores { get; set; } //nadograditi
        public RoundTime RoundTime { get; set; } //selektuje room admin

        public RoomModel()
        {
            Players = new List<UserModel>();
            RoomChat = new List<ChatMessageModel>();
            TeamScores = new Dictionary<String, int>();
            GameState = GameState.Waiting;
            RoundTime = RoundTime.Medium;
        }

        public void ClearChat() 
        { 
            RoomChat?.Clear(); 
        }

        public bool IsReady() //proverava da li prvo ima dovoljno igraca, a onda da li su timovi izjednaceni, tj da li je svako izabrao tim
        {
            if(Players!.Count < 4)
            {
                return false;
            }
            else
            {
                int redTeamCount = 0;
                int blueTeamCount = 0;
                foreach (UserModel player in Players)
                {
                    if (player.TeamId == 1)
                    {
                        redTeamCount++;
                    }
                    else if (player.TeamId == 2)
                    {
                        blueTeamCount++;
                    }
                }
                if (redTeamCount > 2 && blueTeamCount > 2) 
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        
    }
}
