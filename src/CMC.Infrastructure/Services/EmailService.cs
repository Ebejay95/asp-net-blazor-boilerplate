using CMC.Application.Ports;
using Microsoft.Extensions.Logging;

namespace CMC.Infrastructure.Services;

public class EmailService: IEmailService {
  private readonly ILogger<EmailService> _logger;

  public EmailService(ILogger<EmailService> logger) {
    _logger = logger;
  }

  public Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default) {
    // In production, implement actual email sending (SendGrid, SMTP, etc.)
    _logger.LogInformation("Sending password reset email to {Email} with token {Token}", email, resetToken);
    return Task.CompletedTask;
  }

  public Task SendWelcomeEmailAsync(string email, string firstName, CancellationToken cancellationToken = default) {
    _logger.LogInformation("Sending welcome email to {Email} for {FirstName}", email, firstName);
    return Task.CompletedTask;
  }
}
