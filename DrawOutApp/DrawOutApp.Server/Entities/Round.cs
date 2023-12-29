using DrawOutApp.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace DrawOutApp.Server.Entities
{
    public class Round
    {
        [Required]
        public string RoundId { get; set; } 
        public string GameSessionId { get; set; } 
        public int RoundNumber { get; set; } 
        public string ActiveWord { get; set; } 
        public string CurrentPainterId { get; set; } 
        public long RoundStartTime { get; set; } 
        public long RoundEndTime { get; set; } 
        public bool IsStealOpportunityActive { get; set; } 
        public List<ChatMessage> RoundChat { get; set; } 
        public RoundState State { get; set; } 
        public List<DrawingAction> DrawingActions { get; set; }
    }
}
