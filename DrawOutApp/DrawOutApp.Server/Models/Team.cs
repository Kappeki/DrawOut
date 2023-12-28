namespace DrawOutApp.Server.Models
{
    public class Team
    {
        public String TeamId { get; set; }

        public List<Player> Teammates { get; set; }

        public Player TeamLeader { get; set; }

        public int Score { get; set; }

        public List<ChatMessage> TeamChat { get; set; }

        //postoji JEDAN JEDINI teamchat koji prvo koristi tim koji crta, dok ga drugi tim ne vidi. Kada istekne vreme, onda tim koji krade vidi taj cet naravno obrisan skroz, dok sada tim koji je crtao ne vidi cet

        public Team()
        {
            Teammates = new List<Player>();
            TeamChat = new List<ChatMessage>();
        }
        
        public Team(string teamId, List<Player> teammates, Player teamLeader, int score) //ako treba
        {
            TeamId = teamId;
            Teammates = teammates;
            TeamLeader = teamLeader;
            Score = score;
        }

        public void RandomizePlayerOrder()
        {
            Random rnd = new Random();
            Teammates = Teammates.OrderBy(x => rnd.Next()).ToList();
        }
    }
}
