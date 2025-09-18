using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports.Mail;

namespace CMC.Infrastructure.Services;

public sealed class BasicEmailTemplateRenderer : IEmailTemplateRenderer
{
    public Task<(string Subject, string Html, string? Text)> RenderAsync(
        string templateName, object model, CancellationToken ct = default)
    {
        switch (templateName)
        {
            case "PasswordReset":
                // model: { Email, Token, Link }
                dynamic m = model;
                var subject = "Passwort zurücksetzen";
                var html = new StringBuilder()
                    .Append("<p>Hallo,</p>")
                    .Append("<p>Sie haben einen Link zum Zurücksetzen des Passworts angefordert.</p>")
                    .Append($"<p><a href=\"{m.Link}\">Passwort jetzt zurücksetzen</a></p>")
                    .Append("<p>Dieser Link ist 1 Stunde gültig.</p>")
                    .ToString();
                var text = $"Passwort zurücksetzen: {m.Link}\nGültig für 1 Stunde.";
                return Task.FromResult((subject, html, text));

            case "Welcome":
                dynamic w = model;
                var subj2 = "Willkommen bei CMC";
                var html2 = $"<p>Hallo {w.FirstName},</p><p>Willkommen! Viel Erfolg mit CMC.</p>";
                var text2 = $"Hallo {w.FirstName}, Willkommen! Viel Erfolg mit CMC.";
                return Task.FromResult((subj2, html2, text2));

            default:
                return Task.FromResult(($"Mail von CMC", "<p>Hallo!</p>", "Hallo!"));
        }
    }
}
