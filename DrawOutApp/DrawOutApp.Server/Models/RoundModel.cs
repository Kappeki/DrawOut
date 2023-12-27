namespace DrawOutApp.Server.Models
{
    public enum RoundState { InProgress, Finished, StealingOnThose }

    public class RoundModel
    {
        public string RoundId { get; set; }
        public string GameSessionId { get; set; } = null!;
        public int RoundNumber { get; set; }
        public string ActiveWord { get; set; }
        public UserModel CurrentPainter { get; set; }
        public Timer RoundTimer { get; set; }
        public Timer StealTimer { get; set; }
        public bool IsStealOpportunityActive { get; set; }
        public List<ChatMessage>? RoundChat { get; set; }
        public RoundState State { get; set; }
        public List<DrawingAction>? DrawingActions { get; set; }

        //jedinstveni cet za svaki krug, kada istekne vreme, onda se brise
        public RoundModel()
        {
            RoundId = $"round:{GameSessionId}:{RoundNumber}";
        }
        public RoundModel(int roundNumber, string gameSessionId)
        {
            RoundNumber = roundNumber;
            RoundChat = new List<ChatMessage>();
            DrawingActions = new List<DrawingAction>();
            State = RoundState.InProgress; // or other appropriate initial state
            GameSessionId = gameSessionId;
        }

        public void StartRoundTimer(int duration)
        {
            // Logic to start round timer
        }

        public void StartStealTimer(int duration)
        {
            // Logic to start steal timer
        }
        public void ClearRoundChat()
        {
            RoundChat.Clear();
        }

        // Additional methods for round logic (e.g., handling guesses, updating scores)
    }
}