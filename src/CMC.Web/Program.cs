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

var builder = WebApplication.CreateBuilder(args);

// Nur im Development ein paar Bequemlichkeiten (KEIN HTTPS in Prod erzwingen!)
if (builder.Environment.IsDevelopment())
{
    // Kein Zertifikatszwang bei lokalen Calls (nur Dev)
    builder.Services.AddHttpClient("default", client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler
        {
            // Nur in DEV Zertifikatsfehler ignorieren
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        return handler;
    });
}
else
{
    // In Production normaler HttpClient
    builder.Services.AddHttpClient("default", client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });
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

// Revisions-/Papierkorb-Services
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
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
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

// Infrastruktur (AppDbContext, Repos, Queries, ConnectionString via Config/Env)
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRevisionsSupport();

// Bridge für Razor/DI
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());

var app = builder.Build();

// Forwarded Headers (wichtig hinter Ingress für korrekte Scheme/RemoteIP)
var fwdOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
fwdOptions.KnownNetworks.Clear();
fwdOptions.KnownProxies.Clear();
app.UseForwardedHeaders(fwdOptions);

// Pipeline
if (!app.Environment.IsDevelopment())
{
    // HSTS nur als Header für die Browser; TLS endet am Ingress (X-Forwarded-Proto = https)
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    Console.WriteLine("🔧 Development-Hinweis: nutze http://localhost:5000 oder https mit dev-certs.");
}

// KEIN UseHttpsRedirection hier nötig (TLS-Termination macht der Ingress)
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapRazorPages();
app.MapFallbackToPage("/_Host");

// DB-Migration beim Start
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
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
