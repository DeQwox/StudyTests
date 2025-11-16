using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StudyTests.Data;
using Repositories;
using StudyTests.Models.Entities;
using StudyTests.Repositories;
using StudyTests.Data.IdentityServer;
using Duende.IdentityServer;
using StudyTests.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSession();

// Services
builder.Services.AddControllersWithViews();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? "Data Source=StudyTests.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<ITestingRepository, TestingRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ILookupNormalizer, NoOpNormalizer>();

builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;

    if (builder.Environment.IsDevelopment())
    {
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 4;
    }
})
.AddRoles<IdentityRole<int>>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddIdentityServer()
    .AddAspNetIdentity<User>()
    .AddInMemoryApiScopes(Config.ApiScopes)
    .AddInMemoryClients(Config.Clients)
    .AddInMemoryIdentityResources(Config.IdentityResources)
    .AddDeveloperSigningCredential()
    .AddProfileService<CustomProfileService>()
    .AddTestUsers(Config.Users);

builder.Services.AddAuthentication(IdentityServerConstants.DefaultCookieAuthenticationScheme)
    .AddCookie()
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = "https://localhost:5001";
        options.ClientId = "mvc_client";
        options.ClientSecret = "secret";
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.Scope.Add("openid");
        options.Scope.Add("profile");
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<StudyTests.Services.Api.ITestsCrudService, StudyTests.Services.Api.TestsCrudService>();
builder.Services.AddScoped<StudyTests.Services.Api.IQuestionsService, StudyTests.Services.Api.QuestionsService>();
builder.Services.AddScoped<StudyTests.Services.Api.IPassedTestsService, StudyTests.Services.Api.PassedTestsService>();
builder.Services.AddScoped<StudyTests.Services.Api.IUsersService, StudyTests.Services.Api.UsersService>();

// === OpenTelemetry — стабільна і робоча конфігурація ===
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("StudyTests-Service"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation(options =>
        {
            options.SetDbStatementForText = true;                    // ← показує текст SQL
            options.RecordException = true;
        })
        .AddSource("StudyTests.Api")                                 // ← для твоїх ActivitySource
        .AddSource("Microsoft.Data.Sqlite")                          // ← ГОЛОВНЕ! Для Sqlite!
        .AddZipkinExporter(o => 
        {
            o.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
        })
    )
    .WithMetrics(metrics => metrics
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("StudyTests-Service"))
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
    );



var env = builder.Environment;

if (env.IsEnvironment("LoadTest"))
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = null;
        options.DefaultChallengeScheme = null;
    });
}


var app = builder.Build();

// SQLite PRAGMA
var connSection = builder.Configuration.GetSection("ConnectionStrings");
foreach (var conn in connSection.GetChildren())
{
    var connValue = conn.Value;
    if (string.IsNullOrWhiteSpace(connValue) || !connValue.Contains("Data Source")) continue;
    try
    {
        await using var sqliteConn = new SqliteConnection(connValue);
        await sqliteConn.OpenAsync();
        await using var cmd = sqliteConn.CreateCommand();
        cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA busy_timeout = 5000;";
        await cmd.ExecuteNonQueryAsync();
    }
    catch (Exception ex)
    {
        // Log but don't fail startup if PRAGMA can't be set (e.g., running on non-file-based provider or unsupported keywords)
        var tmpLoggerFactory = LoggerFactory.Create(l => l.AddConsole());
        tmpLoggerFactory.CreateLogger("Program").LogWarning(ex, "Could not set PRAGMA for connection '{ConnName}'. Continuing.", conn.Key);
    }
}

app.UseSession();

// Create roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        await SeedRolesAsync(roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error seeding roles");
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseRouting();
app.UseAuthentication();
app.UseIdentityServer();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

/* === ГОЛОВНЕ: метрики на HTTP 5000 — працює завжди === */
app.UseMetricServer("/metrics");                    // додає /metrics
app.UseHttpMetrics();                     // збирає HTTP-метрики





// using (var scope = app.Services.CreateScope())
// {
//     var repo = scope.ServiceProvider.GetRequiredService<ITestingRepository>();

//     var existing = await repo.GetAllTestsAsync();
//     if (!existing.Any())
//     {
//         Console.WriteLine("Seeding tests...");

//         for (int i = 1; i <= 10000; i++)
//         {
//             var model = new StudyTests.Models.DTO.Tests.TestCreateViewModel
//             {
//                 Name = $"Test #{i}",
//                 Description = $"Test description {i}",
//                 Password = "pass",
//                 ValidUntil = DateTime.UtcNow.AddYears(1),
//                 TeacherId = 3, // must exist in DB
//                 Questions =
//                 [
//                     new Question
//                     {
//                         Description = "Q1",
//                         Answers = ["A", "B", "C", "D"],
//                         CorrectAnswerIndex = 0,
//                         Score = 5
//                     }
//                 ]
//             };

//             await repo.AddTestAsync(model);

//             if (i % 500 == 0)
//                 Console.WriteLine($"Seeded {i} tests...");
//         }

//         Console.WriteLine("Seeding complete.");
//     }
// }



app.Run();



static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
{
    string[] roleNames = ["Student", "Teacher"];
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var role = new IdentityRole<int> { Name = roleName, NormalizedName = roleName.ToUpper() };
            await roleManager.CreateAsync(role);
        }
    }
}