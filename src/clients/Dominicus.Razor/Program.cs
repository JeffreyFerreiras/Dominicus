using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IO;
using Dominicus.Models;
using Dominicus.Core.Abstractions;
using Dominicus.Core;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure data protection first (needed for auth and session)
        var keysDirectory = Path.Combine(builder.Environment.ContentRootPath, "keys");
        Directory.CreateDirectory(keysDirectory);

        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory))
            .SetApplicationName("Dominicus");

        // Add core services
        builder.Services.AddRazorPages();
        builder.Services.AddMemoryCache();

        // Configure authentication
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "Dominicus.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromHours(24);
                options.SlidingExpiration = true;
                options.LoginPath = "/Login";
                options.LogoutPath = "/Logout";
                options.AccessDeniedPath = "/AccessDenied";
            });

        // Configure session after authentication
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.Name = "Dominicus.Session"; // Add explicit name
        });

        // Configure application services
        builder.Services.Configure<ClaudeConfig>(
            builder.Configuration.GetSection("ClaudeConfig"));

        builder.Services.AddDistributedMemoryCache(); // Required for session state
        builder.Services.AddScoped<ITranslationService, ClaudeTranslationService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        // Add middleware in the correct order
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        // Authentication and authorization must come before session
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSession();

        app.MapRazorPages();

        app.Run();
    }
}