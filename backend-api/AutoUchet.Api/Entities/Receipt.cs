using System.Text.Json.Serialization;

namespace AutoUchet.Api.Entities
{
    public class Receipt
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string BuyerType { get; set; }
        public string PurposeOfPayment {  get; set; }
        public string Status { get; set; } = "WaitingPayment";
        public string PaymentType { get; set; } = "Auto";
        public string? RobokassaInvoiceId { get; set; }
        public string? MockFnsUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public User? User { get; set; }

    }
}
