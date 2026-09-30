namespace AutoUchet.Api.Services
{
    public static class NotificationHelper
    {
        public static async Task SendAsync(object notificationData)
        {
            try
            {
                var botUrl = AppConfig.BotUrl;
                if (string.IsNullOrEmpty(botUrl)) return;

                var json = System.Text.Json.JsonSerializer.Serialize(notificationData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                var response = await client.PostAsync(botUrl, content);

                if (response.IsSuccessStatusCode)
                    Console.WriteLine($"Уведомление боту отправлено.");
                else
                    Console.WriteLine($"Ошибка бота. Статус: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка уведомления: {ex.Message}");
            }
        }
    }
}