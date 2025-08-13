using CMC.Infrastructure;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using CMC.Contracts.Users;
using CMC.Application.Services;
using CMC.Web.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options => {
  options.DetailedErrors = true;
});

// HttpClient für Blazor Server - OHNE BaseAddress für interne Calls
builder.Services.AddHttpClient();
builder.Services.AddScoped<HttpClient>();

// Session für Session-ID (aber Authentication über InMemory)
builder.Services.AddSession(options => {
  options.IdleTimeout = TimeSpan.FromMinutes(30);
  options.Cookie.HttpOnly = true;
  options.Cookie.IsEssential = true;
  options.Cookie.Name = "CMC_Session";
  options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// PERSISTENT InMemory Authentication - mit statischem Dictionary
builder.Services.AddScoped<PersistentInMemoryAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<PersistentInMemoryAuthenticationStateProvider>());

builder.Services.AddHttpContextAccessor();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment()) {
  app.UseExceptionHandler("/Error");
  app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapRazorPages();
app.MapBlazorHub();

// Logging Middleware
app.Use(async (context, next) => {
  var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
  logger.LogInformation("🌐 Incoming Request: {Method} {Path} from {UserAgent}", context.Request.Method, context.Request.Path, context.Request.Headers.UserAgent.ToString());

  await next();

  logger.LogInformation("📡 Response: {StatusCode} for {Method} {Path}", context.Response.StatusCode, context.Request.Method, context.Request.Path);
});

// KORRIGIERTER Login endpoint
app.MapPost("/api/auth/login", async (HttpContext httpContext, ILogger<Program> logger, UserService userService, PersistentInMemoryAuthenticationStateProvider authProvider) => {

  logger.LogInformation("🎉 LOGIN ENDPOINT REACHED!");

  try {
    // Request Body lesen
    using var reader = new StreamReader(httpContext.Request.Body);
    var body = await reader.ReadToEndAsync();
    logger.LogInformation("📝 Request Body: {Body}", body);

    if (string.IsNullOrEmpty(body)) {
      return Results.BadRequest("Empty request body");
    }

    // JSON parsen
    var loginRequest = JsonSerializer.Deserialize<LoginRequest>(body, new JsonSerializerOptions {
      PropertyNameCaseInsensitive = true
    });

    if (loginRequest == null) {
      return Results.BadRequest("Invalid request format");
    }

    logger.LogInformation("🔐 Login attempt for: {Email}", loginRequest.Email);

    // UserService für Login verwenden
    var user = await userService.LoginAsync(loginRequest);

    if (user != null) {
      // Session ID generieren oder verwenden
      var sessionId = httpContext.Session.Id;

      // In Memory Auth Provider aktualisieren
      authProvider.SetUserSession(sessionId, user.Id.ToString(), user.Email, user.FirstName, user.LastName);

      // Optional: Cookie setzen für Session Tracking
      httpContext.Response.Cookies.Append("CMC_SessionId", sessionId, new CookieOptions {
        HttpOnly = true,
        Secure = false, // true in Production
        SameSite = SameSiteMode.Strict,
        MaxAge = TimeSpan.FromMinutes(30)
      });

      logger.LogInformation("✅ Login successful for: {Email}", loginRequest.Email);

      return Results.Ok(new {
        success = true,
        message = "Login successful",
        user = new {
          id = user.Id,
          email = user.Email,
          firstName = user.FirstName,
          lastName = user.LastName
        }
      });
    } else {
      logger.LogWarning("❌ Login failed for: {Email}", loginRequest.Email);
      return Results.Unauthorized();
    }

  } catch (JsonException ex) {
    logger.LogError(ex, "JSON parsing error");
    return Results.BadRequest("Invalid JSON format");
  } catch (Exception ex) {
    logger.LogError(ex, "Login error");
    return Results.Problem("Internal server error");
  }
});

app.MapPost("/api/auth/logout", async (HttpContext httpContext, ILogger<Program> logger) => {
  try {
    // Session-Daten löschen
    if (httpContext.Session != null) {
      httpContext.Session.Clear();
      await httpContext.Session.CommitAsync();
    }

    logger.LogInformation("✅ User logged out - Session cleared");
    return Results.Ok(new {
      message = "Logout successful"
    });
  } catch (Exception ex) {
    logger.LogError(ex, "Logout error");
    return Results.Problem("Logout failed");
  }
});

// Test endpoints
app.MapGet("/api/test", () => {
  return Results.Ok(new {
    message = "GET endpoint works",
    time = DateTime.Now
  });
});

app.MapPost("/api/test", () => {
  return Results.Ok(new {
    message = "POST endpoint works",
    time = DateTime.Now
  });
});

app.MapFallbackToPage("/_Host");

// Database setup
using(var scope = app.Services.CreateScope()) {
  try {
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();

    try {
      var userService = scope.ServiceProvider.GetRequiredService<UserService>();
      await userService.RegisterAsync(new RegisterUserRequest {
        Email = "test@example.com",
        Password = "password123",
        FirstName = "Test",
        LastName = "User"
      });
    } catch (CMC.Domain.Common.DomainException) {
      // User already exists
    }
  } catch (Exception ex) {
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Database setup failed");
  }
}

app.Run();

public partial class Program {}
