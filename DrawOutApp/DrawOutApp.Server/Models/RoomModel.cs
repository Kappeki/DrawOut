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
        public string? Id { get; set; }
        public string? RoomId { get; set; } = null!;
        public string RoomName { get; set; } = null!;
        public string Password { get; set; } = String.Empty;
        public int PlayerCount { get; set; }   
        public UserModel? RoomAdmin { get; set; }
        public List<UserModel>? Players { get; set; }
        public List<string>? CustomWords { get; set; }
        public List<ChatMessageModel>? RoomChat { get; set; }
        //public ObjectId SelectedWordPack { get; set; }
        public GameState GameState { get; set; }
        public Dictionary<string, int>? TeamScores { get; set; } //nadograditi
        public RoundTime RoundTime { get; set; } //selektuje room admin

        public RoomModel()
        {
            Players = new List<UserModel>();
            RoomChat = new List<ChatMessageModel>();
            TeamScores = new Dictionary<string, int>();
            GameState = GameState.Waiting;
            RoundTime = RoundTime.Medium;
        }

        public void ClearChat() 
        { 
            RoomChat?.Clear(); 
        }

        public bool IsReady()
        {
            // Ensure there are at least 4 players
            if (Players == null || Players.Count < 4)
            {
                return false;
            }

            // Identify unique team IDs
            HashSet<string> uniqueTeamIds = new HashSet<string>();
            foreach (var player in Players)
            {
                uniqueTeamIds.Add(player.TeamId!);
                if (uniqueTeamIds.Count > 2) // More than two unique teams are not allowed
                {
                    return false;
                }
            }

            // If there are not exactly two teams, the game is not ready
            if (uniqueTeamIds.Count != 2)
            {
                return false;
            }

            var teamCounts = new Dictionary<string, int>();
            foreach (var teamId in uniqueTeamIds)
            {
                teamCounts[teamId] = 0;
            }

            // Count the number of players in each team
            foreach (var player in Players)
            {
                if (teamCounts.ContainsKey(player.TeamId!))
                {
                    teamCounts[player.TeamId]++;
                }
            }

            // Check if both teams have more than 2 players
            return teamCounts.All(kvp => kvp.Value > 2);
        }

    }
}
