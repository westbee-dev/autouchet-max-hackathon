using MAX.Bot.Exceptions;
using MAX.Bot.Interfaces;
using MAX.Bot.Interfaces.Models.Request.Message;

namespace Autouchet_Bot.Services
{
    public class TaxReminderBackgroundService : BackgroundService
    {
        private readonly BackendApiClient _apiClient;
        private readonly IMaxBotClient _botClient;
        private readonly ILogger<TaxReminderBackgroundService> _logger;

        public TaxReminderBackgroundService(
            BackendApiClient apiClient,
            IMaxBotClient botClient,
            ILogger<TaxReminderBackgroundService> logger)
        {
            _apiClient = apiClient;
            _botClient = botClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;

                if (TaxDeadlineChecker.IsInTaxNotificationPeriod(now))
                {
                    try
                    {
                        _logger.LogInformation("Запуск плановой рассылки напоминаний о налогах...");

                        var taxSummaries = await _apiClient.GetTaxSummaryAsync();
                        DateTime deadline = TaxDeadlineChecker.GetTaxDeadline(now);

                        foreach (var item in taxSummaries)
                        {
                            if (item.MaxUserId > 0 && item.TaxAmount > 0)
                            {
                                try
                                {
                                    string messageText = $"⏰ **Напоминание об уплате налога!**\n\n" +
                                                         $"Сумма к уплате: **{item.TaxAmount:N2} руб.**\n" +
                                                         $"Крайний срок уплаты: **{deadline:dd.MM.yyyy}**.\n\n" +
                                                         $"Пожалуйста, оплатите налог вовремя.";

                                    await _botClient.SendMessageAsync(new SendMessageRequest
                                    {
                                        UserId = item.MaxUserId,
                                        Text = messageText,
                                        Format = MessageFormat.Markdown
                                    });
                                }

                                catch (MaxBotClientException ex) when (ex.Message.Contains("dialog.not.found"))
                                {
                                    Console.WriteLine($"Пользователь {item.MaxUserId} не открывал чат с ботом.");
                                }
                                
                                   
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка отправке уведомлений.");
                    }
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}