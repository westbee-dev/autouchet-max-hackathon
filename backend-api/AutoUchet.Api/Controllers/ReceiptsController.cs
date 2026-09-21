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
        public async Task<List<ReceiptResponseDto>> GetReceipts()
        {
            var mappedReceipts = await _context.Receipts
                .OrderByDescending(r => r.CreatedAt)
                .Select(
                r => new ReceiptResponseDto
                {
                    Id = r.Id,
                    Amount = r.Amount,
                    BuyerType = r.BuyerType,
                    PurposeOfPayment = r.PurposeOfPayment,
                    Status = r.Status,
                    PaymentType = r.PaymentType,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return mappedReceipts;
        }

        [HttpPost("send-payment-link")]
        public async Task<ActionResult<CreateReceiptResponseDto>> CreateReceiptWithPaymentLink
            ([FromBody] CreateReceiptRequestDto dto)
        {
            var invoiceId = Guid.NewGuid().ToString();
            var paymentUrl = $"https://robokassa.fake/pay?inv={invoiceId}";

            var receipt = new Receipt
            {
                UserId = 1,
                Amount = dto.Amount,
                BuyerType = dto.BuyerType,
                PurposeOfPayment = dto.PurposeOfPayment,
                Status = "WaitingPayment",
                PaymentType = "Auto",
                RobokassaInvoiceId = invoiceId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Receipts.Add(receipt);
            await _context.SaveChangesAsync();

            var response = new CreateReceiptResponseDto
            {
                Id = receipt.Id,
                Status = receipt.Status,
                PaymentUrl = paymentUrl
            };

            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPost("mark-as-paid")]
        public async Task<ActionResult<CreateReceiptResponseDto>> CreateReceiptWithoutPaymentLink
            ([FromBody] CreateReceiptRequestDto dto)
        {
            var mockFnsUrl = Services.MockServices.CreateMockFnsUrl();

            var receipt = new Receipt
            {
                UserId = 1,
                Amount = dto.Amount,
                BuyerType = dto.BuyerType,
                PurposeOfPayment = dto.PurposeOfPayment,
                Status = "Paid",
                PaymentType = "Manual",
                MockFnsUrl = mockFnsUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Receipts.Add(receipt);
            await _context.SaveChangesAsync();

            var response = new CreateReceiptResponseDto
            {
                Id = receipt.Id,
                Status = receipt.Status,
                PaymentUrl = receipt.MockFnsUrl
            };

            return StatusCode(StatusCodes.Status201Created, response);
        }
    }
}
