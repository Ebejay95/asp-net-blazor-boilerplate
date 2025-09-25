using CMC.Infrastructure;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CMC.Application.Ports;
using CMC.Application.Services;
using CMC.Web.Services;
using CMC.Web.Auth;
using CMC.Web.Hubs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CMC.Infrastructure.Extensions;
using CMC.Infrastructure.Services;
using Microsoft.Extensions.Options;
using System.Security.Claims;

// === Helper: Policy-Namen ausgeben (nur fürs Logging/Debugging) ===
static IEnumerable<string> GetPolicyNames(AuthorizationOptions options)
{
    var pi = typeof(AuthorizationOptions).GetProperty("PolicyMap",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    if (pi?.GetValue(options) is IDictionary<string, AuthorizationPolicy> map)
        return map.Keys;
    return Enumerable.Empty<string>();
}

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = !builder.Environment.IsDevelopment();
});

var publicBaseUrl = Environment.GetEnvironmentVariable("APP_PUBLIC_BASE_URL");
var baseUrl = publicBaseUrl ?? "http://localhost:5000";

if (!string.IsNullOrWhiteSpace(publicBaseUrl))
{
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["GraphMail:PublicBaseUrl"] = publicBaseUrl,
        ["BaseUrl"] = publicBaseUrl
    });
    Console.WriteLine($"Public BaseUrl set from env: {publicBaseUrl}");
}

if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("http://localhost:5000");
}

Console.WriteLine($"HttpClient BaseAddress: {baseUrl}");

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.Name = "CMC_Session";
});

builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<HttpClient>(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("api"));

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(o => o.DetailedErrors = builder.Environment.IsDevelopment());
builder.Services.AddControllers();
builder.Services.AddSignalR(o =>
{
    o.EnableDetailedErrors = builder.Environment.IsDevelopment();
    o.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    o.HandshakeTimeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<CookieEvents>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "CMC_Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Cookie.MaxAge = TimeSpan.FromDays(30);
        options.Cookie.Path = "/";
        options.Cookie.IsEssential = true;
        options.LoginPath = "/login";
        options.LogoutPath = "/api/auth/logout";
        options.AccessDeniedPath = "/login";
        options.EventsType = typeof(CookieEvents);
    });

// Authorization Handler registrieren
builder.Services.AddScoped<IAuthorizationHandler, MfaSetupHandler>();
builder.Services.AddScoped<IAuthorizationHandler, MfaVerifiedHandler>();
builder.Services.AddScoped<IAuthorizationHandler, NoMfaVerifiedHandler>();

Console.WriteLine("[Auth] Registering authorization policies...");
builder.Services.AddAuthorization(options =>
{
    Console.WriteLine("[Auth] Adding RequireMfaSetup policy");
    options.AddPolicy("RequireMfaSetup", policy =>
    {
        policy.Requirements.Add(new MfaSetupRequirement());
    });

    Console.WriteLine("[Auth] Adding RequireMfaVerified policy");
    options.AddPolicy("RequireMfaVerified", policy =>
    {
        policy.Requirements.Add(new MfaVerifiedRequirement());
    });

    Console.WriteLine("[Auth] Adding RequireNoMfaVerified policy");
    options.AddPolicy("RequireNoMfaVerified", policy =>
    {
        policy.Requirements.Add(new NoMfaVerifiedRequirement());
    });
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRevisionsSupport();
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<RelationDialogService>();
builder.Services.AddScoped<EFEditService>();

builder.Services.AddScoped<IRelationshipManager, RelationshipManager<AppDbContext>>();
builder.Services.AddScoped<IBumperBus, BumperBus>();

builder.Services.AddScoped<IRevisionKeyResolver, DefaultRevisionKeyResolver>();
builder.Services.AddScoped<IRevisionsClient, EfRevisionsClient>();
builder.Services.AddScoped<CMC.Infrastructure.Services.RevisionService>();
builder.Services.AddScoped<RecycleBinService>();
builder.Services.AddScoped<IRecycleBinClient, RecycleBinClient>();

builder.Services.AddScoped<LibraryScenarioService>();
builder.Services.AddScoped<LibraryControlService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<ReportDefinitionService>();
builder.Services.AddScoped<ScenarioService>();
builder.Services.AddScoped<ControlService>();
builder.Services.AddScoped<RiskAcceptanceService>();
builder.Services.AddScoped<ToDoService>();
builder.Services.AddScoped<EvidenceService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<IndustryService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<INotificationPush, SignalRNotificationPush>();

builder.Services.AddGraphMailServices(builder.Configuration);

var app = builder.Build();

if (args.Any(a => string.Equals(a, "seed-master-user", StringComparison.OrdinalIgnoreCase)))
{
    Console.WriteLine("[seed] Running master-user seeder…");

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync();

    var email = Environment.GetEnvironmentVariable("MASTER_MAIL");
    var hash  = Environment.GetEnvironmentVariable("MASTER_PWHASH");
    var first = Environment.GetEnvironmentVariable("MASTER_FIRST") ?? "MASTER";
    var last  = Environment.GetEnvironmentVariable("MASTER_LAST")  ?? "USER";
    var role  = Environment.GetEnvironmentVariable("MASTER_ROLE")  ?? "super-admin";
    var dept  = Environment.GetEnvironmentVariable("MASTER_DEPT")  ?? "Admin";

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(hash))
    {
        Console.Error.WriteLine("[seed] Missing env MASTER_MAIL or MASTER_PWHASH");
        Environment.Exit(2);
    }

    Console.WriteLine($"[seed] email={email}");

    var id = Guid.NewGuid();

    var updated = await db.Database.ExecuteSqlRawAsync(@"
        UPDATE ""Users""
           SET ""FirstName"" = {1},
               ""LastName""  = {2},
               ""Department""= {3},
               ""Role""      = {4},
               ""IsEmailConfirmed"" = TRUE
         WHERE ""Email"" = {0};
    ", email, first, last, dept, role);

    if (updated == 0)
    {
        await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO ""Users""
              (""Id"",""Email"",""PasswordHash"",""FirstName"",""LastName"",
               ""IsEmailConfirmed"",""CreatedAt"",""Department"",""Role"")
            VALUES
              ({0}, {1}, {2}, {3}, {4}, TRUE, now(), {5}, {6});
        ", id, email, hash, first, last, dept, role);
    }

    Console.WriteLine("[seed] Done.");
    return;
}

var fwd = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
fwd.KnownNetworks.Clear();
fwd.KnownProxies.Clear();
app.UseForwardedHeaders(fwd);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    Console.WriteLine("Dev: Kestrel bindet auf http://localhost:5000");
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    Console.WriteLine("Prod: Kestrel auf konfigurierter URL");
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapRazorPages();
app.MapFallbackToPage("/_Host");

using (var scope = app.Services.CreateScope())
{
    var authOpts = scope.ServiceProvider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;
    var names = GetPolicyNames(authOpts);
    Console.WriteLine("[Auth] Registered policies: " + string.Join(", ", names));
}

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        Console.WriteLine("Database migrations completed");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database setup failed");
    }
}

Console.WriteLine($"Starting CMC application on {baseUrl}");

app.Run();

public partial class Program { }
