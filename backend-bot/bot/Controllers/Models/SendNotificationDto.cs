namespace Autouchet_Bot.Controllers.Models
{
    public class SendNotificationDto
    {
        public long ChatId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}