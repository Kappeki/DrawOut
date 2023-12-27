namespace DrawOutApp.Server.Entities
{
    public class Game
    {
        public string GameSessionId { get; set; } = $"gameSession:{Guid.NewGuid().ToString()}";
        public string RedTeamId { get; set; }
        public string BlueTeamId { get; set; }
        public int TotalRounds { get; set; }
        public List<string> RoundIds { get; set; } // Store RoundModel IDs
        public int CurrentRoundIndex { get; set; }

        public Game()
        {
            RoundIds = new List<string>();
        }
    }
}
