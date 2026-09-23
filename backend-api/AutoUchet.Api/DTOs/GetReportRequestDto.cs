namespace AutoUchet.Api.DTOs
{
    public class GetReportRequestDto
    {
        public long MaxUserId { get; set; }
        public int Quarter { get; set; }
        public int Year { get; set; }
    }
}
