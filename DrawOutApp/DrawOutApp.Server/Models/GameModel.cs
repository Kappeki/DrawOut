using DrawOutApp.Server.Entities;
using System.ComponentModel.DataAnnotations;

namespace DrawOutApp.Server.Models
{
    public class GameRoundModel
    {
        public string _gameId { get; set; } = null!;
        public string? GameState { get; set; }
        public int BlueScore { get; set; }
        public int RedScore { get; set; }
        public int CurrentRound { get; set; }
        public string? CurrentPainter { get; set; }
        public string? SelectedWord { get; set; }
    }

    public class GameModel
    {
        public string _id { get; set; } = null!;
        public string RoomId { get; set; } = null!;
        public int TotalRounds { get; set; }
        public List<string>? PainterOrder { get; set; }
        public Dictionary<string, string>? TeamLeaders { get; set; }
        public int MainTimer { get; set; }
        public int StealTimer { get; set; }
    }
}