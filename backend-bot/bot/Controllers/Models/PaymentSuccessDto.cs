namespace Autouchet_Bot.Controllers.Models
{
    public class PaymentSuccessDto
    {
        public long MaxUserId { get; set; }
        public decimal Amount { get; set; }
        public string? PurposeOfPayment { get; set; }
        public string? InvoiceId { get; set; }
    }
}