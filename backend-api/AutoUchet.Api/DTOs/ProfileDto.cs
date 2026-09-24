namespace AutoUchet.Api.DTOs
{
    public class ProfileDto
    {
        public string FirstName { get; set; }
        public long MaxUserId { get; set; }
        public string ActivityType { get; set; }
        public int ReceiptCountYear { get; set; }
        public decimal TotalIncomeYear { get; set; }
    }
}
