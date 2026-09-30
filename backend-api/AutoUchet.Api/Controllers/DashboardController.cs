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
            var culture = new CultureInfo("ru-RU");

            var deadlineDate = CreateDeadlineDate(nowDate);
            var taxDate = nowDate.AddMonths(-1);

            if (nowDate.Day > deadlineDate.Day)
            {
                taxDate = nowDate;
                deadlineDate = CreateDeadlineDate(nowDate.AddMonths(1));
            }

            var startOfMonth = new DateTime(taxDate.Year, taxDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfMonth = startOfMonth.AddMonths(1);

            var totalIncomeMonth = await _context.Receipts
                .Where(r => r.UserId == user.Id &&
                            r.Status == "Paid" &&
                            r.PaidAt >= startOfMonth &&
                            r.PaidAt < endOfMonth)
                .SumAsync(r => r.Amount);

            var taxAmount = await _context.Receipts
                .Where(r => r.UserId == user.Id &&
                            r.Status == "Paid" &&
                            r.PaidAt >= startOfMonth &&
                            r.PaidAt < endOfMonth)
                .SumAsync(r => r.Amount * (r.BuyerType == "Физ" ? 0.04m : 0.06m));

            var startOfYear = new DateTime(nowDate.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfYear = startOfYear.AddYears(1);

            var totalIncomeYear = await _context.Receipts
                .Where(r => r.UserId == user.Id &&
                            r.Status == "Paid" &&
                            r.PaidAt >= startOfYear &&
                            r.PaidAt < endOfYear)
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