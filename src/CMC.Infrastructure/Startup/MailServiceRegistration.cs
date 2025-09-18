using CMC.Application.Ports;
using CMC.Application.Ports.Mail;
using CMC.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CMC.Infrastructure.Startup;

public static class MailServiceRegistration
{
    public static IServiceCollection AddMailServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<MailOptions>()
            .Configure(opts =>
            {
                // ENV-Variablen aus Kubernetes-Secret "cmc-mail"
                opts.FromEmail     = config["MAIL_FROM"]        ?? opts.FromEmail;
                opts.FromName      = config["MAIL_FROM_NAME"]   ?? opts.FromName;
                opts.PublicBaseUrl = config["PUBLIC_BASE_URL"]  ?? opts.PublicBaseUrl;

                opts.Smtp.Host      = config["SMTP_HOST"]        ?? opts.Smtp.Host;
                if (int.TryParse(config["SMTP_PORT"], out var port)) opts.Smtp.Port = port;
                opts.Smtp.Username  = config["SMTP_USERNAME"]    ?? opts.Smtp.Username;
                opts.Smtp.Password  = config["SMTP_PASSWORD"]    ?? opts.Smtp.Password;
                if (bool.TryParse(config["SMTP_STARTTLS"], out var tls)) opts.Smtp.UseStartTls = tls;
                if (int.TryParse(config["SMTP_TIMEOUT_MS"], out var timeout)) opts.Smtp.TimeoutMs = timeout;
            });

        services.AddSingleton<IEmailTemplateRenderer, SimpleEmailTemplateRenderer>();
        services.AddScoped<IEmailService, SmtpEmailService>();

        return services;
    }
}
