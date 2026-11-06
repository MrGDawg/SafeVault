using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using SafeVault.Data;
using SafeVault.Models;
using SafeVault.Services;

var builder = WebApplication.CreateBuilder(args);

// Register MVC and the application database service.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<SafeVaultDatabase>();

// Register ASP.NET Core's password-hashing implementation.
builder.Services.AddScoped<
    IPasswordHasher<UserAccount>,
    PasswordHasher<UserAccount>>();

builder.Services.AddScoped<PasswordService>();

// The development seeder creates a controlled local account
// whose password is hashed before database storage.
builder.Services.AddScoped<DevelopmentAccountSeeder>();

// Cookie authentication maintains the authenticated user's
// identity between HTTP requests.
builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

// Authentication must establish identity before authorization
// evaluates access to protected resources.
app.UseAuthentication();
app.UseAuthorization();

// Create the controlled account only in the Development environment.
if (app.Environment.IsDevelopment())
{
    using var scope =
        app.Services.CreateScope();

    var seeder =
        scope.ServiceProvider
            .GetRequiredService<DevelopmentAccountSeeder>();

    seeder.Seed();
}

// Use HomeController.Index as the default application endpoint.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

/// <summary>
/// Exposes the application entry point to the integration-test project.
/// </summary>
public partial class Program
{
}