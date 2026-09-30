namespace Autouchet_Bot.Controllers.Models
{
    public class SendNotificationDto
    {
        public long MaxUserId { get; set; }
        public decimal Amount { get; set; }
        public string Purpose { get; set; } = string.Empty;
    }
}