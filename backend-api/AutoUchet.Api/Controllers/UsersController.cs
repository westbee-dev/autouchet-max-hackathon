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
            var targetDate = nowDate.AddMonths(-1);
            var startOfMonth = new DateTime(targetDate.Year, targetDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
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
            ([FromQuery] long maxUserId)
        {
            var user = await _context.Users
                .Include(u => u.Activities)
                .FirstOrDefaultAsync(u => u.MaxUserId == maxUserId);

            if (user == null) return NotFound();

            var nowDate = DateTime.UtcNow;

            var receiptsQuery = _context.Receipts
                .Where(r => r.UserId == user.Id &&
                            r.Status == "Paid" &&
                            r.CreatedAt.Year == nowDate.Year);

            var totalIncomeYear = receiptsQuery.Sum(r => r.Amount);
            var receiptCountYear = receiptsQuery.Count();

            var activities = user.Activities
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.Id)
                .Select(a => new ActivityDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    IsDefault = a.IsDefault
                })
                .ToList();

            return Ok(new ProfileDto
            {
                FirstName = user.FirstName,
                MaxUserId = user.MaxUserId,
                Activities = activities,
                ReceiptCountYear = receiptCountYear,
                TotalIncomeYear = totalIncomeYear
            });
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

        [HttpGet("activities")]
        public async Task<ActionResult<List<ActivityDto>>> GetUserActivities
            ([FromQuery] long maxUserId)
        {
            var user = await _context.Users
                .Include(u => u.Activities)
                .FirstOrDefaultAsync(u => u.MaxUserId == maxUserId);

            if (user == null) return NotFound();

            var activities = user.Activities
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.Id)
                .Select(a => new ActivityDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    IsDefault = a.IsDefault
                })
                .ToList();

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

        [HttpPost("activities")]
        public async Task<ActionResult<ActivityDto>> AddUserActivity
            ([FromQuery] long maxUserId,
            [FromBody] AddActivityDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Activities)
                .FirstOrDefaultAsync(u => u.MaxUserId == maxUserId);

            if (user == null) return NotFound();

            bool isFirst = !user.Activities.Any();

            var activity = new Activity
            {
                UserId = user.Id,
                Name = dto.Name,
                IsDefault = isFirst
            };

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            return Ok(new ActivityDto
            {
                Id = activity.Id,
                Name = activity.Name,
                IsDefault = activity.IsDefault
            });
        }

        [HttpPut("activities/{id}/default")]
        public async Task<ActionResult> SetDefaultActivity(
            [FromQuery] long maxUserId,
            int id)
        {
            var user = await _context.Users
                .Include(u => u.Activities)
                .FirstOrDefaultAsync(u => u.MaxUserId == maxUserId);

            if (user == null) return NotFound();

            var activity = user.Activities.FirstOrDefault(a => a.Id == id);
            if (activity == null) return NotFound();

            foreach (var a in user.Activities)
            {
                a.IsDefault = (a.Id == id);
            }

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

        [HttpDelete("activities/{id}")]
        public async Task<ActionResult> DeleteActivity(
            [FromQuery] long maxUserId,
            int id)
        {
            var user = await _context.Users
                .Include(u => u.Activities)
                .FirstOrDefaultAsync(u => u.MaxUserId == maxUserId);

            if (user == null) return NotFound();

            var activity = user.Activities.FirstOrDefault(a => a.Id == id);
            if (activity == null) return NotFound();

            if (activity.IsDefault && user.Activities.Count > 1)
            {
                var nextDefault = user.Activities.FirstOrDefault(a => a.Id != id);
                if (nextDefault != null)
                {
                    nextDefault.IsDefault = true;
                }
            }

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
