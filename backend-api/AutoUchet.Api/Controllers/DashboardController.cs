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
        public async Task<ActionResult<DashboardResponseDto>> GetDashboard([FromQuery] long maxUserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.MaxUserId == maxUserId);
            if (user == null) return NotFound();

            var nowDate = DateTime.UtcNow;
            var deadlineDate = CreateDeadlineDate(nowDate);
            var taxDate = nowDate.AddMonths(-1);
            var culture = new CultureInfo("ru-RU");

            if (nowDate.Day > deadlineDate.Day)
            {
                taxDate = taxDate.AddMonths(1);
                deadlineDate = CreateDeadlineDate(nowDate.AddMonths(1));
            }

            var totalIncomeMonth = await _context.Receipts
                .Where(
                r => r.UserId == user.Id &&
                r.CreatedAt.Month == taxDate.Month &&
                r.CreatedAt.Year == taxDate.Year && 
                r.Status == "Paid")
                .SumAsync(r => r.Amount);

            var taxAmount = totalIncomeMonth * user.TaxRate;

            var totalIncomeYear = await _context.Receipts
                .Where(
                r => r.UserId == user.Id &&
                r.Status == "Paid" &&
                r.CreatedAt.Year == nowDate.Year)
                .SumAsync(r => r.Amount);

            var limitPercent = Math.Round(((totalIncomeYear / 2_400_000m) * 100), 1);

            var taxMonth = taxDate.ToString("MMMM", culture);
            var taxPeriodText = $"К уплате за {taxMonth}";
            var deadlineText = $"до {deadlineDate:dd.MM}";

            return Ok(new DashboardResponseDto
            {
                TaxPeriodText = taxPeriodText,
                TaxAmount = taxAmount,
                DeadlineText = deadlineText,
                TotalIncomeYear = totalIncomeYear,
                LimitPercent = limitPercent
            });
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
