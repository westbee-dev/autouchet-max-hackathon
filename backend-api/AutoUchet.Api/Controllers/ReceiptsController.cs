using AutoUchet.Api.Data;
using AutoUchet.Api.DTOs;
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
    }
}
