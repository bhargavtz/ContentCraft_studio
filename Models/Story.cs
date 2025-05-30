using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ContentCraft_studio.Models
{
    public class Story
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public required string UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public required string Prompt { get; set; }

        public required string Content { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
