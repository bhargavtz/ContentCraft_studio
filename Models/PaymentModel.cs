using System;

namespace ContentCraft_studio.Models
{
    public class PaymentModel
    {
        public string UserId { get; set; }
        public string PlanName { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } // "card" or "upi"
        public string UpiId { get; set; }
        public string CardNumber { get; set; }
        public string CardExpiry { get; set; }
        public string CardCvc { get; set; }
        public string TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } // "pending", "success", "failed"
    }
}