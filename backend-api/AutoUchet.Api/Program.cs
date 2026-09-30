using AutoUchet.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var botUrl = builder.Configuration["BotSettings:NotificationUrl"]
             ?? "http://backend-bot:8080";

AutoUchet.Api.Services.AppConfig.BotUrl = botUrl;

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var maxRetries = 10;
    var delay = TimeSpan.FromSeconds(3);

    for (int retry = 1; retry <= maxRetries; retry++)
    {
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            Console.WriteLine($"[DB Migration] Попытка подключения к БД #{retry}...");

            context.Database.Migrate();

            Console.WriteLine("[DB Migration] Успешно! Миграции применены.");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DB Migration] База еще не готова ({ex.Message}). Повтор через {delay.TotalSeconds} сек...");
            if (retry == maxRetries)
            {
                Console.WriteLine("[DB Migration] Превышен лимит попыток подключения к БД.");
                throw;
            }
            Thread.Sleep(delay);
        }
    }
}


app.UseSwagger();
app.UseSwaggerUI();


app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseMiddleware<AutoUchet.Api.Middleware.ExceptionMiddleware>();

app.MapControllers();

app.Run();