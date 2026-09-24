namespace AutoUchet.Api.DTOs
{
    public class ReceiptResponseDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string BuyerType {  get; set; }
        public string PurposeOfPayment { get; set; }
        public decimal TaxRate { get; set; }
        public string Status { get; set; }
        public string PaymentType { get; set; }
        public string? PaymentUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
