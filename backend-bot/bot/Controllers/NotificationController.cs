using Autouchet_Bot.Controllers.Models;
using Autouchet_Bot.Keyboards;
using Autouchet_Bot.Services;
using MAX.Bot.Interfaces;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request.Message;
using MAX.Bot.Interfaces.Models.Request.Message.Attachment;
using MAX.Bot.Interfaces.Models.Request.Message.Attachment.Payloads;
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
            var (shouldSend, daysLeft) = TaxDeadlineChecker.CheckNotificationTrigger(today);

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

                string reminderHeader = daysLeft switch
                {
                    1 => "Завтра крайний срок уплаты налога!**",
                    5 => "До уплаты налога осталось 5 дней**",
                    _ => "**Напоминание об уплате налога!**"
                };

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
        [HttpPost("payment-success")]
        public async Task<IActionResult> PaymentSuccess([FromBody] PaymentSuccessDto dto)
        {
            if (dto.MaxUserId <= 0)
            {
                return BadRequest(new { success = false, error = "Некорректный MaxUserId." });
            }

            string purposeText = string.IsNullOrEmpty(dto.PurposeOfPayment)
                ? "Оплата налога"
                : dto.PurposeOfPayment;

            string messageText;
            string buttonText;

            switch (dto.EventType)
            {
                case "receipt_created_waiting":
                    messageText = $"**Чек создан.**\n" +
                                  $"Сумма: **{dto.Amount:N2} руб.**\n" +
                                  $"Назначение: {purposeText}.";
                    buttonText = "Оплатить";
                    break;

                case "receipt_created_paid":
                case "receipt_paid_via_webhook":
                    messageText = $"**Оплата прошла успешно!**\n" +
                                  $"Сумма: **{dto.Amount:N2} руб.**\n" +
                                  $"Назначение: {purposeText}.";
                    buttonText = "Посмотреть чек";
                    break;

                default:
                    messageText = $"**Уведомление по чеку.**\n" +
                                  $"Сумма: **{dto.Amount:N2} руб.**\n" +
                                  $"Назначение: {purposeText}.";
                    buttonText = "Перейти";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(dto.PaymentUrl))
            {
                try
                {
                    var inlineKeyboard = new InlineKeyboardAttachment
                    {
                        Payload = new InlineKeyboardPayload
                        {
                            Buttons = new List<List<Button>>
                    {
                        new List<Button>
                        {
                            new LinkButton
                            {
                                Text = buttonText,
                                Url = dto.PaymentUrl
                            }
                        }
                    }
                        }
                    };

                    await _botClient.SendMessageAsync(new SendMessageRequest
                    {
                        UserId = dto.MaxUserId,
                        Text = messageText,
                        Format = MessageFormat.Markdown,
                        Attachments = new List<Attachment> { inlineKeyboard }
                    });

                    return Ok(new { success = true, message = "Кнопка оплаты отправлена" });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{dto.PaymentUrl}). Ошибка: {ex.Message}");
                }
            }


            try
            {
                string fullText = string.IsNullOrWhiteSpace(dto.PaymentUrl)
                    ? messageText
                    : $"{messageText}\n\nЧек: {dto.PaymentUrl}";

                await _botClient.SendMessageAsync(new SendMessageRequest
                {
                    UserId = dto.MaxUserId,
                    Text = fullText,
                    Format = MessageFormat.Markdown,
                    Attachments = null
                });

                return Ok(new { success = true, message = "Уведомление отправлено" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MAX API Error] Ошибка отправки: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        private static string BuildDefaultMessage(string purposeText, decimal amount, string? invoiceId)
        {
            var messageText = $"**Оплата прошла!**\n\n" +
                              $"Назначение: **{purposeText}**\n" +
                              $"Сумма: **{amount:N2} руб.**\n";

            if (!string.IsNullOrEmpty(invoiceId))
            {
                messageText += $"🧾 Номер чека/транзакции: `{invoiceId}`\n";
            }

            return messageText + "\nДанные обновлены в системе.";
        }
    }
}