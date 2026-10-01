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
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Autouchet_Bot;

class Bot
{
    static async Task Main(string[] args)
    {
        Env.Load();
        var builder = WebApplication.CreateBuilder(args);

        string botToken = Environment.GetEnvironmentVariable("API_KEY_MAX")
            ?? builder.Configuration["BotSettings:MaxToken"]
            ?? builder.Configuration["MaxToken:MaxToken"]
            ?? throw new InvalidOperationException("API токен не найден");

        string miniAppUrl = Environment.GetEnvironmentVariable("MINI_APP_URL")
            ?? builder.Configuration["BotSettings:MiniAppUrl"]
            ?? "http://127.0.0.1";

        string backendApiUrl = Environment.GetEnvironmentVariable("BackendApi__BaseUrl")
                               ?? builder.Configuration["BackendApi:BaseUrl"]
                               ?? "http://backend-api:8080";



        builder.Services.AddControllers();
        builder.Services.AddHttpClient<BackendApiClient>(client =>
        {
            client.BaseAddress = new Uri(backendApiUrl);
        });
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


        _ = Task.Run(async () =>
        {
            long? marker = null;

            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    await client.PollUpdatesWithCallback(
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
                        marker: marker,
                        types: new List<string>
                        {
                          UpdateTypes.MessageCreated,
                          UpdateTypes.MessageCallback
                        },
                        cancellationToken: cts.Token
                    );
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Polling] Ошибка: {ex.Message}");

                    try
                    {
                        marker = await GetCurrentMarkerAsync(botToken, cts.Token);
                        Console.WriteLine($"[Polling] Проблемный апдейт пропущен, продолжаю с marker={marker}");
                    }
                    catch (Exception markerError)
                    {
                        Console.WriteLine($"[Polling] Не удалось получить marker: {markerError.Message}");
                    }

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(3), cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        });

        await app.RunAsync();
        Console.WriteLine("Нажмите Enter для завершения работы бота...");
        Console.ReadLine();
        cts.Cancel();
    }

    static async Task<long?> GetCurrentMarkerAsync(string token, CancellationToken cancellationToken)
    {
        using var http = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://platform-api2.max.ru/updates?limit=1&timeout=0");
        request.Headers.TryAddWithoutValidation("Authorization", token);

        using var response = await http.SendAsync(request, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        using var document = JsonDocument.Parse(json);

        return document.RootElement.TryGetProperty("marker", out var marker) && marker.TryGetInt64(out var value)
            ? value
            : null;
    }
}


