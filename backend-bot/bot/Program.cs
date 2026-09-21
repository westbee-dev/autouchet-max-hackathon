using Autouchet_Bot.Handlers;
using Autouchet_Bot.Keyboards;
using DotNetEnv;
using MAX.Bot;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request.Message;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Autouchet_Bot;

class Bot
{
    static async Task Main(string[] args)
    {
        Env.Load();
        string botToken = Environment.GetEnvironmentVariable("API_KEY_MAX");
        string miniAppUrl = Environment.GetEnvironmentVariable("MINI_APP_URL");
        var client = new MaxBotClient(botToken);
        var messageHandler = new MessageHandler(miniAppUrl);
        var callbackHandler = new CallbackHandler(miniAppUrl);
        var botInfo = await client.GetMeAsync();
        Console.WriteLine($"Бот запущен: {botInfo.FirstName} (ID: {botInfo.Id})");
        using var cts = new CancellationTokenSource();

        var _ = client.PollUpdatesWithCallback(
            async (update, botClient) =>
            {
                if (update is MessageCreatedUpdate messageCreated)
                {
                    await messageHandler.HandleAsync(messageCreated, client);
                }
                else if (update is MessageCallbackUpdate callbackUpdate)
                {
                    await callbackHandler.HandleAsync(callbackUpdate, client);
                }
            },
            limit: 100,
            timeout: 90,
            types: new List<string> 
            { 
              UpdateTypes.MessageCreated,
              UpdateTypes.MessageCallback
            },
            cancellationToken: cts.Token
        );

        Console.WriteLine("Нажмите Enter для завершения работы бота...");
        Console.ReadLine();
        cts.Cancel();
    }
}


