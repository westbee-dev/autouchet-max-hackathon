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
            receipt.MockFnsUrl = Services.GeneratorMockUrl.CreateMockFnsUrl();
            await _context.SaveChangesAsync();

            await Services.NotificationHelper.SendAsync(new
            {
                maxUserId = receipt.User.MaxUserId,
                amount = receipt.Amount,
                purposeOfPayment = receipt.PurposeOfPayment,
                paymentUrl = receipt.MockFnsUrl,
                eventType = "receipt_paid_via_webhook"
            });

            return Content("OK");
        }
    }
}
