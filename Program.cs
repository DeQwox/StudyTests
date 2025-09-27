using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Data;
using StudyTests.Models.Entities;
using Services;
using Repositories;
using Microsoft.EntityFrameworkCore.Storage;

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

Testing testing = new(1, 1, new TestingRepository(context));

for (int i = 0; i < testing.GetQuestionsCount(); i++)
{
    Question question = testing.GetQuestion();
    Console.WriteLine(question.Description);
    Console.WriteLine(string.Join("\n", question.Answers.Select((i, id) => $"{id}: {i}")));

    Console.WriteLine("Answer: ");
    testing.Answer(Convert.ToInt32(Console.ReadLine()));
    Console.WriteLine();
}

context.PassedTests.Add(await testing.GetResult());
context.SaveChanges();

Console.WriteLine(string.Join(" ", context.PassedTests.Select(i => i.Score)));