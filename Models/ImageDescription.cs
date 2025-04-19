using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ContentCraft_Studio.Models
{
    public class ImageDescription
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ImageDescription()
        {
            Id = string.Empty;
            UserId = string.Empty;
            Description = string.Empty;
        }
    }
}
