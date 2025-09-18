using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports.Mail;

namespace CMC.Infrastructure.Services;

// Minimaler Fallback-Renderer (keine Razor-Views erforderlich).
public sealed class SimpleEmailTemplateRenderer : IEmailTemplateRenderer
{
    public Task<(string Subject, string Html, string? Text)> RenderAsync(
        string template, object model, CancellationToken ct = default)
    {
        switch (template)
        {
            case "PasswordReset":
            {
                dynamic m = model;
                string link = m.Link;
                string subj = "Reset your password";
                string html = $"<p>Click here to reset your password:</p><p><a href=\"{link}\">{link}</a></p>";
                string text = $"Reset your password: {link}";
                return Task.FromResult((subj, html, text));
            }
            case "Welcome":
            {
                dynamic m = model;
                string name = m.FirstName ?? "there";
                string subj = "Welcome to CMC";
                string html = $"<p>Hi {name}, welcome to CMC!</p>";
                string text = $"Hi {name}, welcome to CMC!";
                return Task.FromResult((subj, html, text));
            }
            default:
                return Task.FromResult(($"Notification from CMC", "<p>Hi!</p>", "Hi!"));
        }
    }
}
