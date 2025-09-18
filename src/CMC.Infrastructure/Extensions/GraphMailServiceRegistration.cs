using System;
using CMC.Application.Ports;
using CMC.Application.Ports.Mail;
using CMC.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CMC.Infrastructure.Extensions;

public static class GraphMailServiceRegistration
{
    public static IServiceCollection AddGraphMailServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1) Section "GraphMail" manuell in der Configure-Action binden (kein extra Paket nötig)
        services.Configure<GraphMailOptions>(opt =>
        {
            // appsettings.* → GraphMail-Section auf Options mappen
            var section = configuration.GetSection("GraphMail");
            if (section.Exists())
            {
                // benötigt Microsoft.Extensions.Configuration.Binder (ist i.d.R. ohnehin da)
                section.Bind(opt);
            }

            // 2) ENV-Overrides nur anwenden, wenn NICHT leer/Whitespace
            static string? Env(string key)
            {
                var v = Environment.GetEnvironmentVariable(key);
                return string.IsNullOrWhiteSpace(v) ? null : v;
            }

            // Hierarchische Namen (GraphMail__*) bevorzugen; klassische Fallbacks akzeptieren
            opt.TenantId      = Env("GraphMail__TenantId")      ?? Env("GRAPH_TENANT_ID")       ?? opt.TenantId;
            opt.ClientId      = Env("GraphMail__ClientId")      ?? Env("GRAPH_CLIENT_ID")       ?? opt.ClientId;
            opt.ClientSecret  = Env("GraphMail__ClientSecret")  ?? Env("GRAPH_CLIENT_SECRET")   ?? opt.ClientSecret;
            opt.FromUser      = Env("GraphMail__FromUser")      ?? Env("GRAPH_FROM_USER")       ?? opt.FromUser;
            opt.FromEmail     = Env("GraphMail__FromEmail")     ?? Env("MAIL_FROM")             ?? opt.FromEmail;
            opt.FromName      = Env("GraphMail__FromName")      ?? Env("MAIL_FROM_NAME")        ?? opt.FromName;

            // wichtig: nie mit leerem String "leernullen"
            opt.PublicBaseUrl = Env("GraphMail__PublicBaseUrl") ?? Env("PUBLIC_BASE_URL")       ?? opt.PublicBaseUrl;

            // kleines Nice-to-have: keinen trailing slash
            if (!string.IsNullOrWhiteSpace(opt.PublicBaseUrl))
                opt.PublicBaseUrl = opt.PublicBaseUrl!.TrimEnd('/');
        });

        // 3) Validierung + Fail-fast
        services.AddOptions<GraphMailOptions>()
            .Validate(o => !string.IsNullOrWhiteSpace(o.TenantId),     "GRAPH_TENANT_ID required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.ClientId),     "GRAPH_CLIENT_ID required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.ClientSecret), "GRAPH_CLIENT_SECRET required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.FromUser),     "GRAPH_FROM_USER required")
            .ValidateOnStart();

        services.AddSingleton<IEmailTemplateRenderer, BasicEmailTemplateRenderer>();
        services.AddSingleton<IEmailService, GraphEmailService>();
        return services;
    }
}
