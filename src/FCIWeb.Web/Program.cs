using System;
using FCIWeb.Web.Data;
using FCIWeb.Web.Security;
using FCIWeb.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Non annunciare il server (mitigazione information disclosure).
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// MVC + validazione antiforgery globale su tutte le richieste di modifica.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Autenticazione a cookie (demo). In produzione si può aggiungere Negotiate/Windows.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Denied";
        options.Cookie.Name = "fciweb.demo";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });
builder.Services.AddAuthorization();

// DEMO_MODE: persistente su SQLite per default; impostare FCIWEB_STORE=memory per usare RAM.
var storeMode = builder.Configuration["FCIWEB_STORE"] ?? "sqlite";
if (String.Equals(storeMode, "memory", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IRichiestaStore, InMemoryRichiestaStore>();
}
else
{
    var databasePath = builder.Configuration["FCIWEB_SQLITE_PATH"] ?? "data/fciweb.db";
    var databaseDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(databasePath));
    if (!String.IsNullOrEmpty(databaseDirectory)) System.IO.Directory.CreateDirectory(databaseDirectory);
    builder.Services.AddDbContext<FCIWebContext>(options => options.UseSqlite($"Data Source={databasePath}"));
    builder.Services.AddScoped<IRichiestaRepository, EfRichiestaRepository>();
    builder.Services.AddScoped<IRichiestaStore, EfRichiestaStore>();
}
builder.Services.AddSingleton<DemoUserStore>();

var app = builder.Build();

if (!String.Equals(storeMode, "memory", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FCIWebContext>();
    db.Database.EnsureCreated();
}

app.UseSecurityHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

/// <summary>Esposto per i test di integrazione (WebApplicationFactory).</summary>
public partial class Program { }
