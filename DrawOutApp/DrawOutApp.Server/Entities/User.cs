using DrawOutApp.Server.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DrawOutApp.Server.Entities
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonIgnore]
        public ObjectId _id { get; set; }
         
        public string? Nickname { get; set; }
        public HashSet<Role>? Roles { get; set; }
        public string? Icon { get; set; }
        public string? TeamId { get; set; }


        [BsonIgnore]
        public string _sessionKey { get; set; } = null!;
        [BsonIgnore]
        public string ObjectId { get { return _id.ToString(); } }

        public User()
        {
            _id = MongoDB.Bson.ObjectId.GenerateNewId();
            _sessionKey = $"user:{Guid.NewGuid()}";
            Roles = new HashSet<Role>();
        }
    }
}
