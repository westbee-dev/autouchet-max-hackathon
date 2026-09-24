using AutoUchet.Api.Data;

namespace AutoUchet.Api.DTOs
{
    public class DashboardResponseDto
    {
        public string TaxPeriodText { get; set; }
        public decimal TaxAmount { get; set; }
        public string DeadlineText { get; set; }
        public decimal TotalIncomeYear { get; set; }
        public decimal LimitPercent { get; set; }
    }
}
