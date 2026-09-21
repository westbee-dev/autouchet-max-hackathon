using Autouchet_Bot.Controllers.Models;
using MAX.Bot;
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

        public NotificationController(IMaxBotClient botClient)
        {
            _botClient = botClient;
        }

        /// POST: api/notification/send
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationDto dto)
        {
            if (dto.MaxUserId <= 0)
            {
                return BadRequest(new { success = false, error = "Неверный MaxUserId" });
            }

            try
            {
                // Формируем текст уведомления для пользователя
                string messageText = $"✅ **Оплата успешно получена!**\n\n" +
                                     $"💳 **Сумма:** {dto.Amount} руб.\n" +
                                     $"📝 **Назначение:** {dto.Purpose}";

                await _botClient.SendMessageAsync(new SendMessageRequest
                {
                    ChatId = dto.MaxUserId,
                    Text = messageText,
                    Format = MessageFormat.Markdown
                });

                return Ok(new { success = true, message = "Уведомление отправлено" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}