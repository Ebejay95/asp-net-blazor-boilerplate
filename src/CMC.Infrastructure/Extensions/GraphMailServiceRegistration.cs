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
            .Configure(opt =>
            {
                string? Get(string key) => Environment.GetEnvironmentVariable(key);

                opt.TenantId     = Get("GRAPH_TENANT_ID")   ?? opt.TenantId;
                opt.ClientId     = Get("GRAPH_CLIENT_ID")   ?? opt.ClientId;
                opt.ClientSecret = Get("GRAPH_CLIENT_SECRET") ?? opt.ClientSecret;
                opt.FromUser     = Get("GRAPH_FROM_USER")   ?? opt.FromUser;

                opt.PublicBaseUrl = Get("PUBLIC_BASE_URL")  ?? opt.PublicBaseUrl;

                // optional, rein kosmetisch:
                opt.FromEmail    = Get("MAIL_FROM")        ?? opt.FromEmail;
                opt.FromName     = Get("MAIL_FROM_NAME")   ?? opt.FromName;
            });

        services.AddSingleton<IEmailTemplateRenderer, BasicEmailTemplateRenderer>();
        services.AddSingleton<IEmailService, GraphEmailService>();
        return services;
    }
}
