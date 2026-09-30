namespace AutoUchet.Api.DTOs
{
    public class CreateReceiptResponseDto
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string? PaymentUrl { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
