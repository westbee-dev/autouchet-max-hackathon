using AutoUchet.Api.Data;
using AutoUchet.Api.DTOs;
using AutoUchet.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoUchet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ReceiptsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<ReceiptResponseDto>>> GetReceipts([FromQuery] long maxUserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.MaxUserId == maxUserId);
            if (user == null) return NotFound();

            var mappedReceipts = await _context.Receipts
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReceiptResponseDto
                {
                    Id = r.Id,
                    Amount = r.Amount,
                    BuyerType = r.BuyerType,
                    PurposeOfPayment = r.PurposeOfPayment,
                    TaxRate = r.BuyerType == "Физ" ? 0.04m : 0.06m,
                    Status = Services.ReceiptHelper.GetActualStatus(r.Status, r.CreatedAt),
                    PaymentType = r.PaymentType,
                    CreatedAt = r.CreatedAt,
                    PaidAt = r.PaidAt,
                    PaymentUrl = Services.ReceiptHelper.GetActualStatus(r.Status, r.CreatedAt) == "WaitingPayment" && r.PaymentType == "Auto"
                        ? $"https://robokassa.fake/pay?inv={r.RobokassaInvoiceId}"
                        : r.MockFnsUrl
                })
                .ToListAsync();

            return Ok(mappedReceipts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReceiptResponseDto>> GetReceipt(int id, [FromQuery] long maxUserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.MaxUserId == maxUserId);
            if (user == null) return NotFound();

            var receipt = await _context.Receipts.FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);
            if (receipt == null) return NotFound();

            var actualStatus = Services.ReceiptHelper.GetActualStatus(receipt.Status, receipt.CreatedAt);

            var response = new ReceiptResponseDto
            {
                Id = receipt.Id,
                Amount = receipt.Amount,
                BuyerType = receipt.BuyerType,
                TaxRate = receipt.BuyerType == "Физ" ? 0.04m : 0.06m,
                PurposeOfPayment = receipt.PurposeOfPayment,
                CreatedAt = receipt.CreatedAt,
                PaidAt = receipt.PaidAt,
                Status = actualStatus,
                PaymentType = receipt.PaymentType,
                PaymentUrl = actualStatus == "WaitingPayment"
                    ? $"https://robokassa.fake/pay?inv={receipt.RobokassaInvoiceId}"
                    : receipt.MockFnsUrl
            };

            return Ok(response);
        }

        [HttpGet("export")]
        public async Task<ActionResult<List<ReceiptResponseDto>>> GetReportForReceipts([FromQuery] GetReportRequestDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.MaxUserId == dto.MaxUserId);
            if (user == null) return NotFound();

            var startMonth = (dto.Quarter - 1) * 3 + 1;
            var endMonth = startMonth + 2;

            var reportList = await _context.Receipts
                .Where(r => r.UserId == user.Id &&
                r.Status == "Paid" &&
                r.PaidAt.Value.Year == dto.Year &&
                r.PaidAt.Value.Month >= startMonth &&
                r.PaidAt.Value.Month <= endMonth)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReceiptResponseDto
                {
                    Id = r.Id,
                    Amount = r.Amount,
                    BuyerType = r.BuyerType,
                    PurposeOfPayment = r.PurposeOfPayment,
                    Status = r.Status,
                    PaymentType = r.PaymentType,
                    PaymentUrl = r.MockFnsUrl,
                    CreatedAt = r.CreatedAt,
                    PaidAt = r.PaidAt,
                    TaxRate = r.BuyerType == "Физ" ? 0.04m : 0.06m
                }).ToListAsync();

            return Ok(reportList);
        }

        [HttpPost("send-payment-link")]
        public async Task<ActionResult<CreateReceiptResponseDto>> CreateReceiptWithPaymentLink([FromBody] CreateReceiptRequestDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.MaxUserId == dto.MaxUserId);
            if (user == null) return NotFound();

            var invoiceId = Guid.NewGuid().ToString();
            var paymentUrl = $"https://robokassa.fake/pay?inv={invoiceId}";

            var receipt = new Receipt
            {
                UserId = user.Id,
                Amount = dto.Amount,
                BuyerType = dto.BuyerType,
                PurposeOfPayment = await _context.Activities
                    .Where(a => a.Id == dto.ActivityId && a.UserId == user.Id)
                    .Select(a => a.Name)
                    .FirstOrDefaultAsync() ?? "Без назначения",
                Status = "WaitingPayment",
                PaymentType = "Auto",
                RobokassaInvoiceId = invoiceId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Receipts.Add(receipt);
            await _context.SaveChangesAsync();


            await Services.NotificationHelper.SendAsync(new
            {
                maxUserId = user.MaxUserId,
                amount = receipt.Amount,
                purposeOfPayment = receipt.PurposeOfPayment,
                paymentUrl = paymentUrl,
                eventType = "receipt_created_waiting"
            });

            var response = new CreateReceiptResponseDto
            {
                Id = receipt.Id,
                Status = receipt.Status,
                PaymentUrl = paymentUrl
            };

            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPost("mark-as-paid")]
        public async Task<ActionResult<CreateReceiptResponseDto>> CreateReceiptWithoutPaymentLink([FromBody] CreateReceiptRequestDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.MaxUserId == dto.MaxUserId);
            if (user == null) return NotFound();

            var mockFnsUrl = Services.GeneratorMockUrl.CreateMockFnsUrl();

            var receipt = new Receipt
            {
                UserId = user.Id,
                Amount = dto.Amount,
                BuyerType = dto.BuyerType,
                PurposeOfPayment = await _context.Activities
                    .Where(a => a.Id == dto.ActivityId && a.UserId == user.Id)
                    .Select(a => a.Name)
                    .FirstOrDefaultAsync() ?? "Без назначения",
                Status = "Paid",
                PaymentType = "Manual",
                PaidAt = dto.PaidAt ?? DateTime.UtcNow,
                MockFnsUrl = mockFnsUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Receipts.Add(receipt);
            await _context.SaveChangesAsync();

            await Services.NotificationHelper.SendAsync(new
            {
                maxUserId = user.MaxUserId,
                amount = receipt.Amount,
                purposeOfPayment = receipt.PurposeOfPayment,
                paymentUrl = mockFnsUrl,
                eventType = "receipt_created_paid"
            });

            var response = new CreateReceiptResponseDto
            {
                Id = receipt.Id,
                Status = receipt.Status,
                PaymentUrl = receipt.MockFnsUrl,
                PaidAt = receipt.PaidAt
            };

            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteReceipt(int id, [FromQuery] long maxUserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.MaxUserId == maxUserId);
            if (user == null) return NotFound();

            var deletedReceipt = await _context.Receipts.FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);
            if (deletedReceipt == null) return NotFound();

            _context.Receipts.Remove(deletedReceipt);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}