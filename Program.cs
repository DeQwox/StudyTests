using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StudyTests.Data;
using Repositories;
using StudyTests.Models.Entities;
using StudyTests.Repositories;
using StudyTests.Data.IdentityServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Duende.IdentityServer;
using StudyTests.Services;


var builder = WebApplication.CreateBuilder(args);

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

// Add ASP.NET Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    // Default (production) policy
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    options.User.RequireUniqueEmail = true;
    options.Stores.MaxLengthForKeys = 256;

    options.User.AllowedUserNameCharacters = 
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    options.SignIn.RequireConfirmedAccount = false;

    // Relax password requirements in Development to make manual testing via Swagger easier
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

// IdentityServer
builder.Services.AddIdentityServer(options =>
{
    options.UserInteraction.LoginUrl = "/IdentityServerAccount/Login";
})
.AddAspNetIdentity<User>()
.AddInMemoryApiScopes(Config.ApiScopes)
.AddInMemoryClients(Config.Clients)
.AddInMemoryIdentityResources(Config.IdentityResources)
.AddDeveloperSigningCredential()
.AddProfileService<CustomProfileService>()
.AddTestUsers(Config.Users);

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddAuthentication(IdentityServerConstants.DefaultCookieAuthenticationScheme)
.AddCookie()
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = "https://localhost:5001";
    options.RequireHttpsMetadata = true;


    options.ClientId = "mvc_client";
    options.ClientSecret = "secret";
    options.ResponseType = "code";
    
    options.SaveTokens = true;

    options.Scope.Add("openid");
    options.Scope.Add("profile");

    options.CallbackPath = "/signin-oidc";

    options.ClaimActions.MapUniqueJsonKey("role", "role");
    options.ClaimActions.MapUniqueJsonKey(ClaimTypes.Role, ClaimTypes.Role);
    
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Stores.ProtectPersonalData = false;
    options.User.RequireUniqueEmail = true;
    options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
});

builder.Services.AddAuthorization();

// Register API services
builder.Services.AddScoped<StudyTests.Services.Api.ITestsCrudService, StudyTests.Services.Api.TestsCrudService>();
builder.Services.AddScoped<StudyTests.Services.Api.IQuestionsService, StudyTests.Services.Api.QuestionsService>();
builder.Services.AddScoped<StudyTests.Services.Api.IPassedTestsService, StudyTests.Services.Api.PassedTestsService>();
builder.Services.AddScoped<StudyTests.Services.Api.IUsersService, StudyTests.Services.Api.UsersService>();

var app = builder.Build();
// Ensure SQLite is configured for WAL and a reasonable busy timeout to reduce 'database is locked' errors
// Apply PRAGMA to each configured connection string that looks like a file-based SQLite DB.
var connSection = builder.Configuration.GetSection("ConnectionStrings");
foreach (var conn in connSection.GetChildren())
{
    var connValue = conn.Value;
    if (string.IsNullOrWhiteSpace(connValue)) continue;
    // Only attempt PRAGMA on connection strings that contain a Data Source (file-based SQLite)
    if (!connValue.Contains("Data Source", StringComparison.OrdinalIgnoreCase)) continue;

    try
    {
        await using var sqliteConn = new SqliteConnection(connValue);
        await sqliteConn.OpenAsync();
        await using var cmd = sqliteConn.CreateCommand();
        // Enable WAL journal mode for better concurrency
        cmd.CommandText = "PRAGMA journal_mode=WAL;";
        await cmd.ExecuteNonQueryAsync();
        // Set busy timeout (milliseconds) so SQLite will wait briefly when DB is locked
        cmd.CommandText = "PRAGMA busy_timeout = 5000;";
        await cmd.ExecuteNonQueryAsync();
        await sqliteConn.CloseAsync();
    }
    catch (Exception ex)
    {
        // Log but don't fail startup if PRAGMA can't be set (e.g., running on non-file-based provider or unsupported keywords)
        var tmpLoggerFactory = LoggerFactory.Create(l => l.AddConsole());
        tmpLoggerFactory.CreateLogger("Program").LogWarning(ex, "Could not set PRAGMA for connection '{ConnName}'. Continuing.", conn.Key);
    }
}

// Create roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        await SeedRolesAsync(roleManager);
        await SeedInitialUsersAsync(userManager, roleManager);
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
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
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

static async Task SeedInitialUsersAsync(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
{
    // Create an initial teacher account for local testing if it doesn't exist
    var teacherLogin = "teacher";
    var teacherEmail = "teacher@example.local";
    var teacher = await userManager.FindByNameAsync(teacherLogin) ?? await userManager.FindByEmailAsync(teacherEmail);
    if (teacher is not null) return;

    var newTeacher = new User
    {
        UserName = teacherLogin,
        Login = teacherLogin,
        FullName = "Initial Teacher",
        Email = teacherEmail,
        PhoneNumber = "+380501234567"
    };

    var pwd = "P@ssW0rd1!"; // meets Identity password requirements configured in Program.cs
    var createResult = await userManager.CreateAsync(newTeacher, pwd);
    if (!createResult.Succeeded)
    {
        // Log or ignore in dev; we won't throw to avoid breaking startup
        return;
    }

    // Ensure role exists and add user to role
    var roleName = "Teacher";
    if (!await roleManager.RoleExistsAsync(roleName))
    {
        await roleManager.CreateAsync(new IdentityRole<int> { Name = roleName, NormalizedName = roleName.ToUpper() });
    }
    await userManager.AddToRoleAsync(newTeacher, roleName);

    // Confirm email so the account is active for sign-in flows that require confirmation
    var token = await userManager.GenerateEmailConfirmationTokenAsync(newTeacher);
    await userManager.ConfirmEmailAsync(newTeacher, token);
}