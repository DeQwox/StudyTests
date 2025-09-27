using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Data;

// Зчитування конфігурації
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

// Налаштування DbContext для SQLite
var connectionString = configuration.GetConnectionString("DefaultConnection");
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlite(connectionString)
    .Options;

// Використання контексту
using var context = new ApplicationDbContext(options);

Console.WriteLine("Підключення до Entity Framework успішне!");

// Перевіряємо чи існує база даних
try
{
    if (context.Database.CanConnect())
    {
        Console.WriteLine("✅ Підключення до бази даних успішне!");

        Console.WriteLine(string.Join("\n", context.Tests.Select(i => i.Id)));
    }
    else
    {
        Console.WriteLine("⚠️ Не вдається підключитися до бази даних.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Помилка підключення: {ex.Message}");
}
