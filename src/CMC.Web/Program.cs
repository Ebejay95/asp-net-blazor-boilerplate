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
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// 🔧 SSL Development Fix – nur im Development
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ConfigureHttpsDefaults(httpsOptions => { httpsOptions.ServerCertificate = null; });
    });
}

// Add services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options => { options.DetailedErrors = true; });
builder.Services.AddControllers();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);
});

// HttpClient mit SSL-Bypass nur im Development
builder.Services.AddHttpClient("default", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    if (builder.Environment.IsDevelopment())
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
    return handler;
});

// App-Services (DI)
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<RelationDialogService>();
builder.Services.AddScoped<EFEditService>();

// ✅ RelationshipManager an deinen konkreten DbContext binden
builder.Services.AddScoped<IRelationshipManager, RelationshipManager<AppDbContext>>();

// ✅ DB-gestützte Revisions-/Papierkorb-Services (wie gehabt)
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

// ✅ DB-gestützte Claims-Aktualisierung
builder.Services.AddScoped<IClaimsTransformation, DbBackedClaimsTransformation>();

// Authentication + Cookie Setup
builder.Services.AddScoped<CookieEvents>();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
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

// Infrastruktur (ConnectionString via Config/Env)
// Registriert u. a. AppDbContext
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRevisionsSupport();

// 🔌 WICHTIG: Bridge-Registrierung, damit @inject DbContext Db funktioniert
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());

var app = builder.Build();

// Forwarded Headers (wichtig hinter Nginx für HTTPS/Cookies)
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
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
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
Console.WriteLine("   📡 URLs kommen aus ASPNETCORE_URLS oder appsettings.json");

app.Run();

public partial class Program { }
