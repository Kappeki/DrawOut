using System.ComponentModel.DataAnnotations;

namespace DrawOutApp.Server.Entities
{
    public enum Role { Player, Painter, RoomAdmin, TeamLeader }
    public class User
    {
        [Required]
        public String SessionId { get; set; } = $"user:{Guid.NewGuid().ToString()}";

        public String? GameSessionId { get; set; }

        public String? Nickname { get; set; }

        public HashSet<Role> Roles { get; set; }

        public String? Icon { get; set; } //na kraj

        public int TeamId { get; set; } = 0;

        public User()
        {
            Roles = new HashSet<Role>();
        }
    }
}
