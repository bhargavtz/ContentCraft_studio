using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ContentCraft_studio.Models
{
    public class UserActivity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

        public required string UserId { get; set; }
        public required string ActivityType { get; set; }
        public required string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
