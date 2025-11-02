using Microsoft.AspNetCore.Identity;
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

// Add ASP.NET Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    options.User.RequireUniqueEmail = true;
    options.Stores.MaxLengthForKeys = 256;

    options.User.AllowedUserNameCharacters = 
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    options.SignIn.RequireConfirmedAccount = false;
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

var app = builder.Build();

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