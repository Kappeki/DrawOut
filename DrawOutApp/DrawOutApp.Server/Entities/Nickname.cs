using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DrawOutApp.Server.Entities
{
    public class Nickname
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("value")]
        public string Value { get; set; } = String.Empty;
    }
}
