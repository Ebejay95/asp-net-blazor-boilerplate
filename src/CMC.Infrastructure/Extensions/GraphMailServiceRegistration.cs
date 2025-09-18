using CMC.Application.Ports;
using CMC.Application.Ports.Mail;
using CMC.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CMC.Infrastructure.Extensions;

public static class GraphMailServiceRegistration
{
    public static IServiceCollection AddGraphMailServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<GraphMailOptions>()
            // Falls du eine Section "GraphMail" in appsettings hast, erst binden:
            .Bind(configuration.GetSection("GraphMail"))
            // Und dann ENV-Overrides NUR wenn nicht leer:
            .PostConfigure(opt =>
            {
                static string? Env(string key)
                {
                    var v = Environment.GetEnvironmentVariable(key);
                    return string.IsNullOrWhiteSpace(v) ? null : v;
                }

                // Hierarchisch bevorzugen (passt zu .NET-Standard):
                opt.TenantId      = Env("GraphMail__TenantId")      ?? Env("GRAPH_TENANT_ID")       ?? opt.TenantId;
                opt.ClientId      = Env("GraphMail__ClientId")      ?? Env("GRAPH_CLIENT_ID")       ?? opt.ClientId;
                opt.ClientSecret  = Env("GraphMail__ClientSecret")  ?? Env("GRAPH_CLIENT_SECRET")   ?? opt.ClientSecret;
                opt.FromUser      = Env("GraphMail__FromUser")      ?? Env("GRAPH_FROM_USER")       ?? opt.FromUser;
                opt.FromEmail     = Env("GraphMail__FromEmail")     ?? Env("MAIL_FROM")             ?? opt.FromEmail;
                opt.FromName      = Env("GraphMail__FromName")      ?? Env("MAIL_FROM_NAME")        ?? opt.FromName;

                // >>> entscheidend:
                opt.PublicBaseUrl = Env("GraphMail__PublicBaseUrl") ?? Env("PUBLIC_BASE_URL")       ?? opt.PublicBaseUrl;
            })
            // optional: Fail-fast Validierung
            .Validate(o => !string.IsNullOrWhiteSpace(o.TenantId), "GRAPH_TENANT_ID required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.ClientId), "GRAPH_CLIENT_ID required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.ClientSecret), "GRAPH_CLIENT_SECRET required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.FromUser), "GRAPH_FROM_USER required")
            .ValidateOnStart();

        services.AddSingleton<IEmailTemplateRenderer, BasicEmailTemplateRenderer>();
        services.AddSingleton<IEmailService, GraphEmailService>();
        return services;
    }
}
