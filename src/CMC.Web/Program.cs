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
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CMC.Infrastructure.Extensions;
using CMC.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// DEV: weniger strikt validieren
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = !builder.Environment.IsDevelopment();
});

// Public Base URL aus Env
var publicBaseUrl = Environment.GetEnvironmentVariable("APP_PUBLIC_BASE_URL");
if (!string.IsNullOrWhiteSpace(publicBaseUrl))
{
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["GraphMail:PublicBaseUrl"] = publicBaseUrl,
        ["BaseUrl"] = publicBaseUrl // Für HttpClient BaseAddress
    });
    Console.WriteLine($"🌐 Public BaseUrl set from env: {publicBaseUrl}");
}

// Nur Port 5000, ganz einfach
builder.WebHost.UseUrls("http://localhost:5000");
var baseUrl = "http://localhost:5000";
Console.WriteLine($"🌐 HttpClient BaseAddress: {baseUrl}");

// Session Support für 2FA Flow
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.Name = "CMC_Session";
});

builder.Services.AddHttpClient("default", c =>
{
    c.BaseAddress = new Uri(baseUrl);
    c.Timeout = TimeSpan.FromSeconds(30);
});

// Standard HttpClient für Blazor Components
builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var client = factory.CreateClient("default");

    // Cookies für Session-Management
    var handler = new HttpClientHandler()
    {
        UseCookies = true,
        CookieContainer = new System.Net.CookieContainer()
    };

    var httpClient = new HttpClient(handler)
    {
        BaseAddress = new Uri(baseUrl),
        Timeout = TimeSpan.FromSeconds(30)
    };

    return httpClient;
});

// UI / Framework
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(o => o.DetailedErrors = builder.Environment.IsDevelopment());
builder.Services.AddControllers();
builder.Services.AddSignalR(o =>
{
    o.EnableDetailedErrors = builder.Environment.IsDevelopment();
    o.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    o.HandshakeTimeout = TimeSpan.FromSeconds(30);
});

// AUTH
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
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

// Infrastruktur/Repos/DbContext
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRevisionsSupport();
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());

// App-Services (DI)
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<RelationDialogService>();
builder.Services.AddScoped<EFEditService>();

builder.Services.AddScoped<IRelationshipManager, RelationshipManager<AppDbContext>>();
builder.Services.AddScoped<IBumperBus, BumperBus>();

// Revisions-/Papierkorb
builder.Services.AddScoped<IRevisionKeyResolver, DefaultRevisionKeyResolver>();
builder.Services.AddScoped<IRevisionsClient, EfRevisionsClient>();
builder.Services.AddScoped<CMC.Infrastructure.Services.RevisionService>();
builder.Services.AddScoped<RecycleBinService>();
builder.Services.AddScoped<IRecycleBinClient, RecycleBinClient>();

// Application Services
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

// Mail (Graph + Templates)
builder.Services.AddGraphMailServices(builder.Configuration);

var app = builder.Build();

// Seed-only Modus
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

// Forwarded Headers (Ingress)
var fwd = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
fwd.KnownNetworks.Clear();
fwd.KnownProxies.Clear();
app.UseForwardedHeaders(fwd);

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    Console.WriteLine("🔧 Dev: Kestrel bindet auf http://localhost:5000");
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    Console.WriteLine("🏭 Prod: Kestrel auf http://localhost:5000");
}

app.UseStaticFiles();
app.UseRouting();

// Session BEFORE Auth
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapRazorPages();
app.MapFallbackToPage("/_Host");

// DB-Migration beim regulären Start
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        Console.WriteLine("✅ Database migrations completed");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Database setup failed");
    }
}

Console.WriteLine("🚀 Starting CMC application on http://localhost:5000");

app.Run();

public partial class Program { }
