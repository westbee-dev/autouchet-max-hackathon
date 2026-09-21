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
        /// POST: api/notification/send отправка уведомлений в чат
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationDto dto)
        {
            if (dto.ChatId <= 0 || string.IsNullOrWhiteSpace(dto.Message))
            {
                return BadRequest(new { success = false, error = "Неверный ChatId" });
            }

            try
            {
                await _botClient.SendMessageAsync(new SendMessageRequest
                {
                    ChatId = dto.ChatId,
                    Text = dto.Message,
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