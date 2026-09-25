namespace AutoUchet.Api.DTOs
{
    public class CreateReceiptRequestDto
    {
        public long MaxUserId { get; set; }
        public string BuyerType { get; set; }
        public decimal Amount { get; set; }
        public int ActivityId { get; set; }
    }
}