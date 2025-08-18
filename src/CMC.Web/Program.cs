using CMC.Infrastructure;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CMC.Contracts.Users;
using CMC.Application.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using CMC.Application.Ports;

var builder = WebApplication.CreateBuilder(args);

// 🔧 SSL Development Fix - Nur für Development
if (builder.Environment.IsDevelopment()) {
  builder.WebHost.ConfigureKestrel(options => {
    options.ConfigureHttpsDefaults(httpsOptions => {
      httpsOptions.ServerCertificate = null; // Verwendet Development-Zertifikat
    });
  });
}

// Kestrel URLs explizit setzen
builder.WebHost.UseUrls("http://localhost:5000", "https://localhost:5001");

// Add services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options => {
  options.DetailedErrors = true;
});

// 🔧 API Controllers hinzufügen
builder.Services.AddControllers();

// 🔧 SignalR für Blazor konfigurieren
builder.Services.AddSignalR(options => {
  options.EnableDetailedErrors = true;
  options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
  options.HandshakeTimeout = TimeSpan.FromSeconds(30);
});

// HttpClient mit SSL-Bypass für Development
builder.Services.AddHttpClient("default", client => {
  client.Timeout = TimeSpan.FromSeconds(30);
}).ConfigurePrimaryHttpMessageHandler(() => {
  var handler = new HttpClientHandler();
  if (builder.Environment.IsDevelopment()) {
    // SSL-Zertifikat-Validierung für Development ausschalten
    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
  }
  return handler;
});

// 🔧 Authentication Fix - KORREKTE Cookie-Konfiguration für echte Persistenz
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => {
  options.Cookie.Name = "CMC_Auth";
  options.Cookie.HttpOnly = true;
  options.Cookie.SameSite = SameSiteMode.Lax;
  options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
    ? CookieSecurePolicy.SameAsRequest
    : CookieSecurePolicy.Always;

  // 🔧 KRITISCH für echte Persistenz - das ist der Schlüssel!
  options.ExpireTimeSpan = TimeSpan.FromDays(30);
  options.SlidingExpiration = true;
  options.Cookie.MaxAge = TimeSpan.FromDays(30);
  options.Cookie.Path = "/";
  options.Cookie.IsEssential = true;

  // 🔧 WICHTIG: Diese Einstellung macht Cookies persistent über Browser-Restarts
  options.Events.OnSigningIn = context => {
    context.Properties.IsPersistent = true;
    context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30);
    Console.WriteLine(
      "🔐 User signing in: " + context.Principal
      ?.Identity
        ?.Name);
    return Task.CompletedTask;
  };

  options.LoginPath = "/login";
  options.LogoutPath = "/api/auth/logout"; // 🔧 API Route verwenden
  options.AccessDeniedPath = "/login";

  options.Events.OnSignedIn = context => {
    Console.WriteLine(
      "✅ User signed in: " + context.Principal
      ?.Identity
        ?.Name);
    return Task.CompletedTask;
  };

  options.Events.OnValidatePrincipal = context => {
    Console.WriteLine(
      "🔍 Validating principal: " + context.Principal
      ?.Identity
        ?.Name);
    if (
      context.Principal
      ?.Identity
        ?.IsAuthenticated == true) {
      Console.WriteLine("✅ Principal is valid and authenticated");
    } else {
      Console.WriteLine("❌ Principal validation failed");
      context.RejectPrincipal();
    }
    return Task.CompletedTask;
  };
});

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment()) {
  app.UseExceptionHandler("/Error");
  app.UseHsts();
} else {
  app.UseDeveloperExceptionPage();

  // 🔧 Development SSL Trust Helper
  Console.WriteLine("🔧 Development SSL Info:");
  Console.WriteLine("   Falls SSL-Fehler auftreten, führe aus:");
  Console.WriteLine("   dotnet dev-certs https --trust");
  Console.WriteLine("   Oder nutze HTTP: http://localhost:5000");
}

app.UseStaticFiles();
app.UseRouting();

// 🔧 KRITISCH: Authentication MUSS vor Authorization stehen
app.UseAuthentication();
app.UseAuthorization();

// 🔧 API Controllers VOR Blazor
app.MapControllers();

// 🔧 Blazor Hub
app.MapBlazorHub();

// 🔧 WICHTIG: Reihenfolge der Mappings
app.MapRazorPages();
app.MapFallbackToPage("/_Host");

// Database setup
using(var scope = app.Services.CreateScope()) {
  try {
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
    Console.WriteLine("✅ Database migrations completed");
  } catch (Exception ex) {
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "❌ Database setup failed");
  }
}

Console.WriteLine("🚀 Starting CMC application...");
Console.WriteLine("   📡 Available at:");
Console.WriteLine("      http://localhost:5000 (empfohlen für Development)");
Console.WriteLine("      https://localhost:5001 (requires trusted certificate)");
Console.WriteLine("   🔧 Bei SSL-Problemen: dotnet dev-certs https --trust");

app.Run();

public partial class Program {}
