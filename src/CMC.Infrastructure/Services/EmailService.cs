using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Application.Ports.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CMC.Infrastructure.Services;

public sealed class EmailService : IEmailService
{
    private readonly MailOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<MailOptions> options, ILogger<EmailService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger  = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("email is required", nameof(email));
        if (string.IsNullOrWhiteSpace(resetToken)) throw new ArgumentException("resetToken is required", nameof(resetToken));

        // Reset-Link
        var baseUrl = (_options.PublicBaseUrl ?? "").TrimEnd('/');
        var link = string.IsNullOrWhiteSpace(baseUrl)
            ? $"/reset-password?token={Uri.EscapeDataString(resetToken)}"
            : $"{baseUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";

        var subject = "Reset your password";
        var text = $@"You requested a password reset.

If you made this request, click the link below to reset your password:
{link}

If you didn't request this, you can ignore this email.";
        var html = $@"<p>You requested a password reset.</p>
<p>If you made this request, click the button below to reset your password:</p>
<p><a href=""{link}"" style=""display:inline-block;padding:10px 16px;border-radius:6px;background:#2563eb;color:#fff;text-decoration:none"">Reset password</a></p>
<p>If you didn't request this, you can ignore this email.</p>";

        await SendAsync(email, subject, html, text, cancellationToken);
    }

    public async Task SendWelcomeEmailAsync(string email, string firstName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("email is required", nameof(email));

        var subject = "Welcome to CMC";
        var safeName = string.IsNullOrWhiteSpace(firstName) ? "there" : firstName;
        var text = $@"Hi {safeName},

welcome to CMC! You can now sign in and start using the app.

Cheers,
Your CMC Team";
        var html = $@"<p>Hi {WebUtility.HtmlEncode(safeName)},</p>
<p>welcome to <strong>CMC</strong>! You can now sign in and start using the app.</p>
<p>Cheers,<br/>Your CMC Team</p>";

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

        // Plain-Text Alternative (optional)
        if (!string.IsNullOrWhiteSpace(textBody))
        {
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(textBody!, null, "text/plain"));
        }

        using var smtp = new SmtpClient(_options.Smtp.Host, _options.Smtp.Port)
        {
            EnableSsl = _options.Smtp.UseStartTls, // For Outlook/M365: true with port 587
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(_options.Smtp.Username))
        {
            smtp.Credentials = new NetworkCredential(_options.Smtp.Username, _options.Smtp.Password);
        }

        try
        {
            _logger.LogInformation("📨 Sending mail to {To} via {Host}:{Port}", to, _options.Smtp.Host, _options.Smtp.Port);
            // SmtpClient has no native cancellation; we wrap in Task.Run to honor ct.
            await Task.Run(() => smtp.Send(msg), ct);
            _logger.LogInformation("✅ Mail sent to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Sending email to {To} failed", to);
            throw; // lass die Exception nach oben, damit dein UI "Server error" zeigt
        }
    }
}
