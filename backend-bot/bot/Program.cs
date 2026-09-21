using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotNetEnv;
using MAX.Bot;

using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request.Message;

namespace MyMaxBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Env.Load();
            string botToken = Environment.GetEnvironmentVariable("API_KEY_MAX");

            // 1. Создание клиента с токеном
            var botClient = new MaxBotClient(botToken);

            // 2. Получение информации о боте
            var botInfo = await botClient.GetMeAsync();
            Console.WriteLine($"Бот запущен: {botInfo.FirstName} (ID: {botInfo.Id})");

            using var cts = new CancellationTokenSource();

            // 3. Запуск фонового получения обновлений (PollUpdatesWithCallback)
            var _ = botClient.PollUpdatesWithCallback(
                async (update, client) =>
                {
                    // Проверяем, что обновление — это входящее сообщение
                    if (update is MessageCreatedUpdate messageCreated && messageCreated.Message != null)
                    {
                       
                        var msg = messageCreated.Message;

                        // Извлекаем текст и ID чата
                        string incomingText = msg.Body?.Text;
                        var currentUserId = msg.Sender.Id;  // Динамически получаем ChatId отправителя
                        //long currentChatId = msg.Re
                        
                        Console.WriteLine($"[Чат {currentUserId}]: {incomingText}");

                        if (!string.IsNullOrEmpty(incomingText))
                        {
                            await client.SendMessageAsync(new SendMessageRequest
                            {
                                UserId = currentUserId, // Указываем UserId для личного сообщения
                                ChatId = null,          // Обязательно null, чтобы API не искало групповой чат
                                Text = $"Привет! Ваш ответ на сообщение: {incomingText}"
                            });
                        }
                    }
                },
                limit: 100,
                timeout: 90,
                types: new List<string> { UpdateTypes.MessageCreated },
                cancellationToken: cts.Token
            );

            Console.WriteLine("Нажмите Enter для завершения работы бота...");
            Console.ReadLine();
            cts.Cancel();
        }
    }
}