namespace DrawOutApp.Server.Models
{
    public class GameSession
    {
        public String GameSessionId { get; set; }
        public Team RedTeam { get; set; }
        public Team BlueTeam { get; set; }
        public int TotalRounds { get; private set; } = 8;
        public List<Round> Rounds { get; private set; }
        public int CurrentRoundIndex { get; private set; }
        public Round CurrentRound => Rounds[CurrentRoundIndex];

        public GameSession()
        {
            RedTeam = new Team();
            BlueTeam = new Team();
            Rounds = new List<Round>(TotalRounds);
            InitializeRounds();
        }

        private void InitializeRounds()
        {
            for (int i = 0; i < TotalRounds; i++)
            {
                Rounds.Add(new Round(i + 1));
            }
        }

        public void StartNextRound()
        {
            if (CurrentRoundIndex < TotalRounds - 1)
            {
                CurrentRoundIndex++;
                // Additional logic to start the round (e.g., set CurrentPainter, ActiveWord, etc.)
            }
            else
            {
                // Handle end of the game
            }
        }

        // Additional methods to handle game logic, score updates, etc.
    }
}