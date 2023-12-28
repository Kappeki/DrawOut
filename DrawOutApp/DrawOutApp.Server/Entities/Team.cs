namespace DrawOutApp.Server.Entities
{
    public class Team
    {
        public string TeamId { get; set; } = $"team:{Guid.NewGuid().ToString()}";
        public List<string> TeammateIds { get; set; } 
        public string TeamLeaderId { get; set; }
        public string GameSessionId { get; set; }
        public int Score { get; set; }

        public Team()
        {
            TeammateIds = new List<string>();
        }
    }
}
