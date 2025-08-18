using CMC.Infrastructure;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CMC.Contracts.Users;
using CMC.Application.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// 🔧 SSL Development Fix – nur im Development
if (builder.Environment.IsDevelopment()) {
  builder.WebHost.ConfigureKestrel(options => {
    options.ConfigureHttpsDefaults(httpsOptions => {
      httpsOptions.ServerCertificate = null;
    });
  });
}

// Add services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options => {
  options.DetailedErrors = true;
});

builder.Services.AddControllers();

builder.Services.AddSignalR(options => {
  options.EnableDetailedErrors = true;
  options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
  options.HandshakeTimeout = TimeSpan.FromSeconds(30);
});

// HttpClient mit SSL-Bypass nur im Development
builder.Services.AddHttpClient("default", client => {
  client.Timeout = TimeSpan.FromSeconds(30);
}).ConfigurePrimaryHttpMessageHandler(() => {
  var handler = new HttpClientHandler();
  if (builder.Environment.IsDevelopment()) {
    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
  }
  return handler;
});

// Authentication + Cookie Setup
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => {
  options.Cookie.Name = "CMC_Auth";
  options.Cookie.HttpOnly = true;
  options.Cookie.SameSite = SameSiteMode.Lax;
  options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
    ? CookieSecurePolicy.SameAsRequest
    : CookieSecurePolicy.Always;

  options.ExpireTimeSpan = TimeSpan.FromDays(30);
  options.SlidingExpiration = true;
  options.Cookie.MaxAge = TimeSpan.FromDays(30);
  options.Cookie.Path = "/";
  options.Cookie.IsEssential = true;

  options.LoginPath = "/login";
  options.LogoutPath = "/api/auth/logout";
  options.AccessDeniedPath = "/login";

  options.Events.OnSigningIn = context => {
    context.Properties.IsPersistent = true;
    context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30);
    Console.WriteLine(
      "🔐 User signing in: " + context.Principal
      ?.Identity
        ?.Name);
    return Task.CompletedTask;
  };
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
      Console.WriteLine("✅ Principal valid");
    } else {
      Console.WriteLine("❌ Principal invalid");
      context.RejectPrincipal();
    }
    return Task.CompletedTask;
  };
});

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

// Infrastruktur (zieht ConnectionString aus Config / Env)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Forwarded Headers (wichtig hinter Nginx für HTTPS/Cookies)
var fwdOptions = new ForwardedHeadersOptions {
  ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
  KnownNetworks = {},
  KnownProxies = {}
};
app.UseForwardedHeaders(fwdOptions);

// Pipeline
if (!app.Environment.IsDevelopment()) {
  app.UseExceptionHandler("/Error");
  app.UseHsts();
} else {
  app.UseDeveloperExceptionPage();
  Console.WriteLine("🔧 Development SSL Info:");
  Console.WriteLine("   dotnet dev-certs https --trust");
  Console.WriteLine("   oder nutze http://localhost:5000");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapRazorPages();
app.MapFallbackToPage("/_Host");

// Datenbank-Migration
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
Console.WriteLine("   📡 URLs kommen aus ASPNETCORE_URLS oder appsettings.json");

app.Run();

public partial class Program {}
