using System;
using CMC.Application.Ports.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CMC.Infrastructure.Extensions;

public static class MailOptionsConfiguration
{
    /// Erwartete Env Vars:
    /// MAIL_FROM, MAIL_FROM_NAME, SMTP_HOST, SMTP_PORT, SMTP_USERNAME, SMTP_PASSWORD, SMTP_STARTTLS, PUBLIC_BASE_URL
    public static IServiceCollection AddMailOptionsFromEnvironment(
        this IServiceCollection services,
        IConfiguration configuration,
        bool allowConfigFallback = true)
    {
        services.AddOptions<MailOptions>()
            .Configure<IConfiguration>((opt, cfg) =>
            {
                if (allowConfigFallback)
                {
                    var mail = cfg.GetSection("Mail");
                    if (mail.Exists())
                    {
                        opt.FromEmail     = mail["FromEmail"]     ?? opt.FromEmail;
                        opt.FromName      = mail["FromName"]      ?? opt.FromName;
                        opt.PublicBaseUrl = mail["PublicBaseUrl"] ?? opt.PublicBaseUrl;

                        var smtp = mail.GetSection("Smtp");
                        if (smtp.Exists())
                        {
                            opt.Smtp.Host = smtp["Host"] ?? opt.Smtp.Host;
                            if (int.TryParse(smtp["Port"], out var cfgPort)) opt.Smtp.Port = cfgPort;
                            opt.Smtp.Username = smtp["Username"] ?? opt.Smtp.Username;
                            opt.Smtp.Password = smtp["Password"] ?? opt.Smtp.Password;
                            if (bool.TryParse(smtp["UseStartTls"], out var cfgStartTls)) opt.Smtp.UseStartTls = cfgStartTls;
                        }
                    }
                }

                string? Get(string key) => Environment.GetEnvironmentVariable(key);

                opt.FromEmail      = Get("MAIL_FROM")        ?? opt.FromEmail;
                opt.FromName       = Get("MAIL_FROM_NAME")   ?? opt.FromName;
                opt.PublicBaseUrl  = Get("PUBLIC_BASE_URL")  ?? opt.PublicBaseUrl;

                opt.Smtp.Host      = Get("SMTP_HOST")        ?? opt.Smtp.Host;
                if (int.TryParse(Get("SMTP_PORT"), out var port)) opt.Smtp.Port = port;
                opt.Smtp.Username  = Get("SMTP_USERNAME")    ?? opt.Smtp.Username;
                opt.Smtp.Password  = Get("SMTP_PASSWORD")    ?? opt.Smtp.Password;
                if (bool.TryParse(Get("SMTP_STARTTLS"), out var startTls)) opt.Smtp.UseStartTls = startTls;
            });

        return services;
    }
}
