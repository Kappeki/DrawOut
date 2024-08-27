using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using DrawOutApp.Server.Entities;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace DrawOutApp.Server.Models
{
    public class PlayerInfo
    {
        public string? Nickname { get; set; }
        public string? Icon { get; set; }
    }
    public class UserModel
    {
        public string _sessionKey { get; private set; } = null!;
        public string? Nickname { get; set; }
        public List<string>? Roles { get; set; } 
        public string? Icon { get; set; }
    }
    public record class UserPreferences(string? Nickname, string? Icon);
}
