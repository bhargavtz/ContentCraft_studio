using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ContentCraft_studio.Models
{
    public class PaymentModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public required string UserId { get; set; }

        public required string PlanName { get; set; }

        public required decimal Amount { get; set; }

        public string? TransactionId { get; set; }

        public PaymentStatus Status { get; set; }

        [BsonElement("PaymentDate")]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }
}
