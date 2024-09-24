using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DrawOutApp.Server.Entities
{
    public class Icon
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("image_name")]
        public string ImageName { get; set; } = String.Empty;

        [BsonElement("image_data")]
        public string ImageData { get; set; } = String.Empty;
    }
}
