using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ContentCraft_studio.Models
{
    public class BusinessNameModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("UserId")]
        public string? UserId { get; set; }

        [BsonElement("Name")]
        public string? Name { get; set; }

        [BsonElement("BrandIdentity")]
        public BrandIdentity? BrandIdentity { get; set; }

        [BsonElement("NameMeaning")]
        public string? NameMeaning { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("Timestamp")]
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; } = string.Empty;

        public BusinessNameModel()
        {
            Timestamp = DateTime.UtcNow;
        }
    }

    public class BrandIdentity
    {
        [BsonElement("UniquePoints")]
        public string[]? UniquePoints { get; set; }

        [BsonElement("IndustryFit")]
        public string? IndustryFit { get; set; }

        [BsonElement("Keywords")]
        public string[]? Keywords { get; set; }
    }
}
