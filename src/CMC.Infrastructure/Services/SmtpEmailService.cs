using System.Net;
using System.Net.Mail;
using CMC.Application.Ports;
using CMC.Application.Ports.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CMC.Infrastructure.Services;

public sealed class SmtpEmailService : IEmailService
{
    private readonly MailOptions _options;
    private readonly IEmailTemplateRenderer _renderer;
    private readonly ILogger<SmtpEmailService> _log;

    public SmtpEmailService(
        IOptions<MailOptions> options,
        IEmailTemplateRenderer renderer,
        ILogger<SmtpEmailService> log)
    {
        _options  = options.Value ?? new MailOptions();
        _renderer = renderer;
        _log      = log;
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken ct = default)
    {
        var baseUrl = (_options.PublicBaseUrl ?? "").TrimEnd('/');
        var link = string.IsNullOrWhiteSpace(baseUrl)
            ? $"/reset-password?token={Uri.EscapeDataString(resetToken)}"
            : $"{baseUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";

        var (subject, html, text) =
            await _renderer.RenderAsync("PasswordReset", new { Email = email, Token = resetToken, Link = link }, ct);

        await SendAsync(email, subject, html, text, ct);
    }

    public async Task SendWelcomeEmailAsync(string email, string firstName, CancellationToken ct = default)
    {
        var (subject, html, text) =
            await _renderer.RenderAsync("Welcome", new { Email = email, FirstName = firstName }, ct);

        await SendAsync(email, subject, html, text, ct);
    }

    private async Task SendAsync(string to, string subject, string htmlBody, string? textBody, CancellationToken ct)
    {
        _log.LogInformation("📧 Preparing email: to={To} host={Host} port={Port} starttls={Tls} from={From} name={Name}",
            to, _options.Smtp.Host, _options.Smtp.Port, _options.Smtp.UseStartTls, _options.FromEmail, _options.FromName);

        using var msg = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        msg.To.Add(to);

        if (!string.IsNullOrWhiteSpace(textBody))
        {
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(textBody!, null, "text/plain"));
        }

        using var smtp = new SmtpClient(_options.Smtp.Host, _options.Smtp.Port)
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            EnableSsl = _options.Smtp.UseStartTls, // Für 587 (STARTTLS=true). Für MailHog false.
            Timeout = _options.Smtp.TimeoutMs
        };

        if (!string.IsNullOrWhiteSpace(_options.Smtp.Username))
            smtp.Credentials = new NetworkCredential(_options.Smtp.Username, _options.Smtp.Password);

        try
        {
            _log.LogDebug("Connecting to SMTP…");
            await smtp.SendMailAsync(msg, ct);
            _log.LogInformation("✅ Mail sent to {To}", to);
        }
        catch (SmtpException sx)
        {
            _log.LogError(sx, "❌ SMTP error while sending to {To} (StatusCode may be unavailable on .NET Core)", to);
            throw;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "❌ Sending email to {To} failed", to);
            throw;
        }
    }
}
