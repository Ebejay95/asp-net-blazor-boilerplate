using CMC.Infrastructure;
using CMC.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options => {
  options.DetailedErrors = true;
  options.DisconnectedCircuitMaxRetained = 100;
  options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
  options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
  options.MaxBufferedUnacknowledgedRenderBatches = 10;
});

// Configure SignalR for Docker
builder.Services.AddSignalR(options => {
  options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
  options.HandshakeTimeout = TimeSpan.FromSeconds(30);
  options.KeepAliveInterval = TimeSpan.FromSeconds(15);
  options.MaximumReceiveMessageSize = 32 * 1024; // 32KB
});

// Add authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => {
  options.LoginPath = "/login";
  options.LogoutPath = "/logout";
  options.ExpireTimeSpan = TimeSpan.FromDays(30);
  options.SlidingExpiration = true;
  options.Cookie.Name = "AuthCookie";
  options.Cookie.HttpOnly = true;
  options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Allow HTTP in development
});

builder.Services.AddAuthorization();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment()) {
  app.UseExceptionHandler("/Error");
  app.UseHsts();
}

// Remove HTTPS redirection for Docker container
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub(options => {
  options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets | Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
});
app.MapFallbackToPage("/_Host");

// Run migrations automatically - AKTIVIERT für Docker
using(var scope = app.Services.CreateScope()) {
  try {
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("=== Starting database migration ===");

    // Retry logic for database connection
    var retryCount = 0;
    var maxRetries = 10;

    while (retryCount < maxRetries) {
      try {
        var canConnect = await context.Database.CanConnectAsync();
        logger.LogInformation("Can connect to database: {CanConnect}", canConnect);

        if (canConnect) {
          // Get pending migrations
          var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
          logger.LogInformation("Pending migrations: {PendingMigrations}", string.Join(", ", pendingMigrations));

          // Apply migrations
          await context.Database.MigrateAsync();

          // Verify migrations applied
          var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
          logger.LogInformation("Applied migrations: {AppliedMigrations}", string.Join(", ", appliedMigrations));

          logger.LogInformation("=== Database migration completed successfully ===");
          break;
        }
      } catch (Exception dbEx) {
        retryCount++;
        logger.LogWarning("Database connection attempt {Attempt}/{MaxAttempts} failed: {Error}", retryCount, maxRetries, dbEx.Message);

        if (retryCount >= maxRetries) {
          throw;
        }

        await Task.Delay(2000); // Wait 2 seconds before retry
      }
    }
  } catch (Exception ex) {
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "=== ERROR: Database migration failed ===");
    logger.LogError("Exception Type: {ExceptionType}", ex.GetType().Name);
    logger.LogError("Exception Message: {Message}", ex.Message);
    if (ex.InnerException != null) {
      logger.LogError("Inner Exception: {InnerException}", ex.InnerException.Message);
    }

    // DON'T throw - let the app start anyway for debugging
    logger.LogWarning("=== Continuing without database - app will have limited functionality ===");
  }
}

app.Run();

// Diese Zeile muss ganz am Ende stehen:
public partial class Program {}
