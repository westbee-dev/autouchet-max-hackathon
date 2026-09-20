using AutoUchet.Api.Data;
using AutoUchet.Api.DTOs;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoUchet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<DashboardResponseDto> GetDashboard()
        {
            var nowDate = DateTime.UtcNow;
            var deadlineDate = CreateDeadlineDate(nowDate);
            var taxDate = nowDate.AddMonths(-1);
            var culture = new CultureInfo("ru-RU");

            if (nowDate.Day > deadlineDate.Day)
            {
                taxDate = taxDate.AddMonths(1);
                deadlineDate = CreateDeadlineDate(nowDate.AddMonths(1));
            }
            
            var taxAmount = await _context.Receipts
                .Where(
                r => r.CreatedAt.Month == taxDate.Month &&
                r.CreatedAt.Year == taxDate.Year && 
                r.Status == "Paid")
                .SumAsync(r =>  r.Amount);

            var totalIncomeYear = await _context.Receipts
                .Where(
                r => r.Status == "Paid" &&
                r.CreatedAt.Year == taxDate.Year)
                .SumAsync(r => r.Amount);

            var limitPercent = Math.Round(((totalIncomeYear / 2_400_000m) * 100), 1);

            var taxPeriodText = $"К уплате за {tax}";
            var deadlineText = 

        }

        private static DateTime CreateDeadlineDate(DateTime nowDate)
        {
            var deadlineDate = new DateTime(nowDate.Year, nowDate.Month, 28);

            if (deadlineDate.DayOfWeek == DayOfWeek.Saturday)
            {
                deadlineDate = deadlineDate.AddDays(2);
            }
            else if (deadlineDate.DayOfWeek == DayOfWeek.Sunday)
            {
                deadlineDate = deadlineDate.AddDays(1);
            }

            return deadlineDate;
        }
    }
}
