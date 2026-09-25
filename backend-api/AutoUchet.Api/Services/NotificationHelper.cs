namespace AutoUchet.Api.Services
{
    public static class NotificationHelper
    {
        private static readonly string _botUrl = "http://26.7.68.242:5232/api/Notification/payment-success";

        public static async Task SendAsync(object notificationData)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(notificationData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                var response = await client.PostAsync(_botUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ Уведомление боту отправлено успешно.");
                }
                else
                {
                    Console.WriteLine($"❌ Ошибка при отправке боту. Статус: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка уведомления бота: {ex.Message}");
            }
        }
    }
}
