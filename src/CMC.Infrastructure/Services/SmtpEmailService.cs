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
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IOptions<MailOptions> options,
        IEmailTemplateRenderer renderer,
        ILogger<SmtpEmailService> logger)
    {
        _options  = options.Value ?? new MailOptions();
        _renderer = renderer;
        _logger   = logger;
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default)
    {
        var baseUrl = (_options.PublicBaseUrl ?? "").TrimEnd('/');
        var link = string.IsNullOrWhiteSpace(baseUrl)
            ? $"/reset-password?token={Uri.EscapeDataString(resetToken)}"
            : $"{baseUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";

        var (subject, html, text) =
            await _renderer.RenderAsync("PasswordReset", new { Email = email, Token = resetToken, Link = link }, ct: cancellationToken);

        await SendAsync(email, subject, html, text, cancellationToken);
    }

    public async Task SendWelcomeEmailAsync(string email, string firstName, CancellationToken cancellationToken = default)
    {
        var (subject, html, text) =
            await _renderer.RenderAsync("Welcome", new { Email = email, FirstName = firstName }, ct: cancellationToken);

        await SendAsync(email, subject, html, text, cancellationToken);
    }

    private async Task SendAsync(string to, string subject, string htmlBody, string? textBody, CancellationToken ct)
    {
        using var msg = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        msg.To.Add(to);

        // Text-Alternative (optional)
        if (!string.IsNullOrWhiteSpace(textBody))
        {
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(textBody!, null, "text/plain"));
        }

        using var smtp = new SmtpClient(_options.Smtp.Host, _options.Smtp.Port)
        {
            EnableSsl = _options.Smtp.UseStartTls
        };

        if (!string.IsNullOrWhiteSpace(_options.Smtp.Username))
        {
            smtp.Credentials = new NetworkCredential(_options.Smtp.Username, _options.Smtp.Password);
        }

        try
        {
            await smtp.SendMailAsync(msg, ct);
            _logger.LogInformation("📧 Mail sent to {To} via {Host}:{Port}", to, _options.Smtp.Host, _options.Smtp.Port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Sending email to {To} failed", to);
            throw;
        }
    }
}
