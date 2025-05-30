using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ContentCraft_studio.Models
{
    public class BlogPost
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string UserId { get; set; } = null!;

        public string UserName { get; set; } = string.Empty;

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }

         public BlogPost()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}
