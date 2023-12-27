namespace DrawOutApp.Server.Models
{
    public enum RoundState { InProgress, Finished, StealingOnThose }

public class Round
    {
        public int RoundNumber { get; set; }
        public string ActiveWord { get; set; }
        public Player CurrentPainter { get; set; }
        public Timer RoundTimer { get; set; }
        public Timer StealTimer { get; set; }
        public bool IsStealOpportunityActive { get; set; }
        public List<ChatMessage> TeamChatMessages { get; set; }
        public RoundState State { get; set; }

        public Round(int roundNumber)
        {
            RoundNumber = roundNumber;
            TeamChatMessages = new List<ChatMessage>();
            State = RoundState.InProgress; // or other appropriate initial state
        }

        public void StartRoundTimer(int duration)
        {
            // Logic to start round timer
        }

        public void StartStealTimer(int duration)
        {
            // Logic to start steal timer
        }

        // Additional methods for round logic (e.g., handling guesses, updating scores)
    }
}