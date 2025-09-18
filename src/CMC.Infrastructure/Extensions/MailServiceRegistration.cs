using CMC.Application.Ports;
using CMC.Application.Ports.Mail;
using CMC.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CMC.Infrastructure.Extensions;

public static class MailServiceRegistration
{
    /// <summary>
    /// Registriert MailOptions (aus Env/K8s), den Template-Renderer und den SMTP-Service.
    /// </summary>
    public static IServiceCollection AddMailServices(
        this IServiceCollection services,
        IConfiguration configuration,
        bool allowConfigFallback = true)
    {
        services.AddMailOptionsFromEnvironment(configuration, allowConfigFallback);
        services.AddSingleton<IEmailTemplateRenderer, BasicEmailTemplateRenderer>();
        services.AddSingleton<IEmailService, GraphEmailService>();
        return services;
    }
}
