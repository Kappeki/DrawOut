using DrawOutApp.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace DrawOutApp.Server.Entities
{
    public class User
    {
        [Required]
        public string SessionId { get; set; } = $"user:{Guid.NewGuid().ToString()}";

        public string? GameSessionId { get; set; }

        public string? Nickname { get; set; }

        public HashSet<Role> Roles { get; set; }

        public string? Icon { get; set; } 

        public string? TeamId { get; set; }

        public User()
        {
            Roles = new HashSet<Role>();
        }
    }
}
