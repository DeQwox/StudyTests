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


// Add ASP.NET Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
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
    
});

builder.Services.AddAuthorization();

var app = builder.Build();

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