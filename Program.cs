using Microsoft.EntityFrameworkCore;
using Data;
using Repositories;
using StudyTests.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// Add Swagger/OpenAPI generator (so UseSwagger() has the required services)
builder.Services.AddSwaggerGen();

// Configure DbContext from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=StudyTests.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Register repository and services
builder.Services.AddScoped<ITestingRepository, TestingRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // In development show detailed errors and enable Swagger UI
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Додаємо middleware для анти-CSRF
app.UseAntiforgery();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();