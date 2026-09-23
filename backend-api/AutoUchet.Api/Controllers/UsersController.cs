using AutoUchet.Api.Data;
using AutoUchet.Api.DTOs;
using AutoUchet.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoUchet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("tax-summary")]
        public async Task<List<TaxSummaryDto>> GetUsersTaxSummary()
        {
            var nowDate = DateTime.UtcNow;
            var startOfMonth = new DateTime(nowDate.Year, nowDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfMonth = startOfMonth.AddMonths(1);

            var summary = await _context.Users
                .Where(u => u.RemindAboutTax == true)
                .Select(u => new TaxSummaryDto
                {
                    MaxUserId = u.MaxUserId,
                    TaxAmount = (u.Receipts
                    .Where(r => r.Status == "Paid" && r.PaidAt >= startOfMonth && r.PaidAt < endOfMonth)
                    .Sum(r => (decimal?)r.Amount) ?? 0m) * u.TaxRate
                })
                .Where(u => u.TaxAmount > 0)
                .ToListAsync();

            return summary;

        }

        [HttpGet("profile")]
        public async Task<ActionResult<ProfileDto>> GetUserProfile
            ([FromQuery] long MaxUserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.MaxUserId == MaxUserId);
            if (user == null) return NotFound();

            var nowDate = DateTime.UtcNow;

            var receiptsForYearList = _context.Receipts
                .Where(r => r.UserId == user.Id &&
                r.Status == "Paid" &&
                r.CreatedAt.Year == nowDate.Year);

            var totalIncomeYear = receiptsForYearList.Sum(r => r.Amount);

            int receiptCountYear = receiptsForYearList.Count();

            var response = new ProfileDto
            {
                FirstName = user.FirstName,
                MaxUserId = user.MaxUserId,
                ActivityType = user.ActivityType,
                ReceiptCountYear = receiptCountYear,
                TotalIncomeYear = totalIncomeYear
            };
            return Ok(response);
        }

        [HttpGet("settings")]
        public async Task<ActionResult<SettingsDto>> GetUserSettings
            ([FromQuery] long MaxUserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.MaxUserId == MaxUserId);
            if (user == null) return NotFound();

            return Ok(
                new SettingsDto
                {
                    RemindAboutTax = user.RemindAboutTax,
                });
        }

        // пока заглушка, создать таблицу в бд нужно, у каждого пользователя может быть много видов активностей
        [HttpGet("activities")]
        public async Task<ActionResult<List<string>>> GetUserActivities([FromQuery] long maxUserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.MaxUserId == maxUserId);
            if (user == null) return NotFound();

            var activities = string.IsNullOrEmpty(user.ActivityType)
                ? new List<string>()
                : new List<string> { user.ActivityType };

            return Ok(activities);
        }

        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> CreateUser
            (CreateUserRequestDto dto)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.MaxUserId == dto.MaxUserId);
            if (existingUser != null)
            {
                return Ok(new UserResponseDto
                {
                    Id = existingUser.Id,
                    MaxUserId = existingUser.MaxUserId,
                    FirstName = existingUser.FirstName
                });
            }

            var newUser = new User
            {
                MaxUserId = dto.MaxUserId,
                FirstName = dto.FirstName,
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var response = new UserResponseDto
            {
                Id = newUser.Id,
                MaxUserId = newUser.MaxUserId,
                FirstName = newUser.FirstName
            };

            return StatusCode(StatusCodes.Status201Created, response);
        }

        // пока заглушка, создать таблицу в бд нужно, у каждого пользователя может быть много видов активностей
        [HttpPut("activity")]
        public async Task<ActionResult> UpdateUserActivity
            ([FromQuery] long maxUserId, [FromBody] UpdateActivityDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.MaxUserId == maxUserId);
            if (user == null) return NotFound();

            user.ActivityType = dto.ActivityType;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("settings")]
        public async Task<ActionResult<SettingsDto>> PutUserSettings
            ([FromQuery] long MaxUserId, SettingsDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.MaxUserId == MaxUserId);
            if (user == null) return NotFound();

            user.RemindAboutTax = dto.RemindAboutTax;
            await _context.SaveChangesAsync();

            return Ok(
            new SettingsDto
            {
                RemindAboutTax = user.RemindAboutTax,
            });
        }
    }
}
