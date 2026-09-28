using AutoUchet.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

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
                Console.WriteLine("[DB Migration] Превышено максимальное количество попыток подключения к БД.");
                throw;
            }
            Thread.Sleep(delay);
        }
    }
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();