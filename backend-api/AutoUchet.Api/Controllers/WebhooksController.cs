using AutoUchet.Api.Data;
using AutoUchet.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoUchet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhooksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WebhooksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("robokassa")]
        public async Task<ActionResult> CreateRobokassaWebhook
            ([FromBody] RobokassaWebhookDto webhook)
        {
            var receipt = await _context.Receipts
                .Include(r => r.User)
                .FirstOrDefaultAsync
                (r => r.RobokassaInvoiceId == webhook.InvoiceId);

            if (receipt == null)
            {
                return NotFound();
            }

            if (receipt.Status == "Paid")
            {
                return Content("OK");
            }

            receipt.Status = "Paid";
            receipt.PaidAt = DateTime.UtcNow;
            receipt.MockFnsUrl = Services.MockServices.CreateMockFnsUrl();
            await _context.SaveChangesAsync();

            //try 
            //{
            //    var botUrl = "https://inclusive-suggestion-weddings-thee.trycloudflare.com/api/notification/send";
            //    var notificationData = new 
            //    { 
            //        maxUserId = receipt.User.MaxUserId, 
            //        amount = receipt.Amount, 
            //        purpose = receipt.PurposeOfPayment 
            //    };

            //    var json = System.Text.Json.JsonSerializer.Serialize(notificationData);
            //    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            //    using var client = new HttpClient();
            //    await client.PostAsync(botUrl, content);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Ошибка уведомления бота: {ex.Message}");
            //}

            Console.WriteLine($"[ЗАГЛУШКА] Уведомление боту для MaxUserId: {receipt.User?.MaxUserId}, Сумма: {receipt.Amount}");
            return Content("OK");
        }
    }
}
