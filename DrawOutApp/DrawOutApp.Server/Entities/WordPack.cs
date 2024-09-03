using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DrawOutApp.Server.Entities
{
    public class WordPack
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = String.Empty;

        [BsonElement("words")]
        public List<string> Words { get; set; } = new();
    }
}
