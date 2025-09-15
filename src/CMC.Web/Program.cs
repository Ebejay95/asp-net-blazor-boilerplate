// src/CMC.Web/Program.cs
using CMC.Infrastructure;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CMC.Application.Services;
using CMC.Web.Services;
using CMC.Web.Auth;
using CMC.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// --- DEV: Kestrel:Endpoints aus der Konfiguration neutralisieren + Ports fest auf 5000/5001 ---
if (builder.Environment.IsDevelopment())
{
    // Alle Kestrel-Endpunkte “überschreiben”, damit der Konfig-Loader später nichts auf 8080 bindet
    var killKestrel = new Dictionary<string, string?>
    {
        ["Kestrel:Endpoints:Http:Url"] = null,
        ["Kestrel:Endpoints:Https:Url"] = null,
        ["Kestrel:Endpoints:Https:Certificate:Path"] = null,
        ["Kestrel:Endpoints:Https:Certificate:Password"] = null
    };
    builder.Configuration.AddInMemoryCollection(killKestrel);

    // URLs aus Env ignorieren
    Environment.SetEnvironmentVariable("ASPNETCORE_URLS", null);

    builder.WebHost.ConfigureKestrel(o =>
    {
        o.ListenLocalhost(5000); // HTTP
        try { o.ListenLocalhost(5001, lo => lo.UseHttps()); } catch { /* kein dev cert -> ok */ }
    });
}

// HttpClient
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpClient("default", c => c.Timeout = TimeSpan.FromSeconds(30))
        .ConfigurePrimaryHttpMessageHandler(() =>
            new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true });
}
else
{
    builder.Services.AddHttpClient("default", c => c.Timeout = TimeSpan.FromSeconds(30));
}

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

// App-Services (DI)
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<RelationDialogService>();
builder.Services.AddScoped<EFEditService>();

// Bridge für @inject DbContext Db
builder.Services.AddScoped<IRelationshipManager, RelationshipManager<AppDbContext>>();

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

// Auth
builder.Services.AddScoped<CookieEvents>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "CMC_Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // hinter Ingress ok
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

// Infrastruktur (DbContext usw.)
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRevisionsSupport();
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());

var app = builder.Build();

// --- Seed-only Modus: führt Seeding aus und beendet den Prozess ---
if (args.Any(a => string.Equals(a, "seed-master-user", StringComparison.OrdinalIgnoreCase)))
{
    Console.WriteLine("[seed] Running master-user seeder…");

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // DB sicher auf dem aktuellen Stand
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

    // 1) Update, falls User existiert
    var updated = await db.Database.ExecuteSqlRawAsync(@"
        UPDATE ""Users""
           SET ""FirstName"" = {1},
               ""LastName""  = {2},
               ""Department""= {3},
               ""Role""      = {4},
               ""IsEmailConfirmed"" = TRUE
         WHERE ""Email"" = {0};
    ", email, first, last, dept, role);

    // 2) Insert, falls nicht vorhanden
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
    return; // WICHTIG: keinen Webserver starten
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
    Console.WriteLine("🔧 Dev: Kestrel bindet auf http://localhost:5000 und (falls Zertifikat) https://localhost:5001.");
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    Console.WriteLine("🏭 Prod/Test: Kestrel/URLs kommen aus Config/Env (z. B. http://0.0.0.0:8080 hinter Ingress).");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
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

Console.WriteLine("🚀 Starting CMC application...");
Console.WriteLine("   📡 URLs kommen aus Kestrel-Endpunkten (Production: http://0.0.0.0:8080) oder ASPNETCORE_URLS.");

app.Run();

public partial class Program { }
