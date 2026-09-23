using Autouchet_Bot.Services;
using MAX.Bot.Interfaces;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request.Message;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Autouchet_Bot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IMaxBotClient _botClient;
        private readonly BackendApiClient _apiClient;

        public NotificationController(IMaxBotClient botClient, BackendApiClient apiClient)
        {
            _botClient = botClient;
            _apiClient = apiClient;
        }

        
        [HttpPost("send-tax-reminders")]
        public async Task<IActionResult> SendTaxReminders([FromQuery] bool forceSend = false)
        {
            DateTime today = DateTime.Today;

            if (!forceSend && !TaxDeadlineChecker.IsInTaxNotificationPeriod(today))
            {
                return Ok(new
                {
                    success = true,
                    message = $"Сегодня ({today:dd.MM.yyyy}) не входит в период оповещения о налогах (с 25 по {TaxDeadlineChecker.GetTaxDeadline(today):dd.MM.yyyy})."
                });
            }

            try
            {
                var taxSummaries = await _apiClient.GetTaxSummaryAsync();

                if (taxSummaries == null || taxSummaries.Count == 0)
                {
                    return Ok(new { success = true, message = "Нет активных начислений по налогам." });
                }

                DateTime deadline = TaxDeadlineChecker.GetTaxDeadline(today);
                int sentCount = 0;
                int failedCount = 0;

                foreach (var item in taxSummaries)
                {
                    if (item.MaxUserId > 0 && item.TaxAmount > 0)
                    {
                        try
                        {


                            string messageText = $" **Напоминание об уплате налога!**\n\n" +
                                                 $"Сумма к уплате: **{item.TaxAmount:N2} руб.**\n" +
                                                 $"Крайний срок уплаты: **{deadline:dd.MM.yyyy}**.\n\n" +
                                                 $"Пожалуйста, оплатите налог вовремя, чтобы избежать начисления пени.";

                            await _botClient.SendMessageAsync(new SendMessageRequest
                            {
                                UserId = item.MaxUserId,
                                Text = messageText,
                                Format = MessageFormat.Markdown
                            });

                            sentCount++;
                        }
                        catch (Exception ex)
                        {
                            failedCount++;
                            Console.WriteLine($"[Warning] Пропуск MaxUserId={item.MaxUserId}: {ex.Message}");
                        }
                    }
                }

                return Ok(new { success = true, sentReminders = sentCount, deadline = deadline.ToString("yyyy-MM-dd") });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}