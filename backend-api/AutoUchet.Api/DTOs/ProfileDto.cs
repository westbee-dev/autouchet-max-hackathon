namespace AutoUchet.Api.DTOs
{
    public class ProfileDto
    {
        public string FirstName { get; set; }
        public long MaxUserId { get; set; }
        public List<ActivityDto> Activities { get; set; } = new();
        public int ReceiptCountYear { get; set; }
        public decimal TotalIncomeYear { get; set; }
    }
}