using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ContentCraft_studio.Models
{
    public class Caption
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public required string UserId { get; set; }

        [Required(ErrorMessage = "Caption text is required")]
        [StringLength(2000, ErrorMessage = "Caption text must not exceed 2000 characters")]
        public required string Text { get; set; }

        public required string Mood { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
