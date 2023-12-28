namespace DrawOutApp.Server.Models
{
    public class TeamModel
    {
        public String TeamId { get; set; } 

        public List<UserModel> Teammates { get; set; }

        public UserModel TeamLeader { get; set; }

        public int Score { get; set; }

        public string GameSessionId { get; set; }

        //postoji JEDAN JEDINI teamchat koji prvo koristi tim koji crta, dok ga drugi tim ne vidi. Kada istekne vreme, onda tim koji krade vidi taj cet naravno obrisan skroz, dok sada tim koji je crtao ne vidi cet

        public TeamModel()
        {
            Teammates = new List<UserModel>();
        }
        
        public TeamModel(string teamId, List<UserModel> teammates, UserModel teamLeader, int score) //ako treba
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
        //zove se kad se izadje iz sobe
        public void ClearTeam()
        {
            Teammates.Clear();
        }
    }
}
