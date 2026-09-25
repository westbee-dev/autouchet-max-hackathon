using Autouchet_Bot.Handlers;
using Autouchet_Bot.Keyboards;
using Autouchet_Bot.Services;
using DotNetEnv;
using MAX.Bot;
using MAX.Bot.Interfaces;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request.Message;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddHttpClient<BackendApiClient>();
        builder.Services.AddHostedService<TaxReminderBackgroundService>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        var client = new MaxBotClient(botToken);

        builder.Services.AddSingleton<IMaxBotClient>(client);    

        builder.Services.AddSingleton(sp => new MessageHandler(miniAppUrl));
        builder.Services.AddSingleton(sp =>
            new CallbackHandler(miniAppUrl, sp.GetRequiredService<BackendApiClient>()));

        var app = builder.Build();
        app.MapControllers();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        var messageHandler = app.Services.GetRequiredService<MessageHandler>();
        var callbackHandler = app.Services.GetRequiredService<CallbackHandler>();

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
        await app.RunAsync();
        Console.WriteLine("Нажмите Enter для завершения работы бота...");
        Console.ReadLine();
        cts.Cancel();
    }
}


