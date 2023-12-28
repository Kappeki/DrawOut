using DrawOutApp.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace DrawOutApp.Server.Entities
{
    public class Round
    {
        [Required]
        public string RoundId { get; set; } 
        public string GameSessionId { get; set; } // Reference to the game session
        public int RoundNumber { get; set; } // The number of the round within the game
        public string ActiveWord { get; set; } // The word that players need to guess
        public string CurrentPainterId { get; set; } // ID of the user who is drawing
        public long RoundStartTime { get; set; } // Timestamp when the round started
        public long RoundEndTime { get; set; } // Timestamp when the round ended or when the steal opportunity started
        public bool IsStealOpportunityActive { get; set; } // Indicates if the steal opportunity is active
        public List<ChatMessage> RoundChat { get; set; } // Chat messages for this round
        public RoundState State { get; set; } // State of the round (e.g., InProgress, Finished)
        public List<DrawingAction> DrawingActions { get; set; } // Drawing actions that occurred during this round
    }
}
