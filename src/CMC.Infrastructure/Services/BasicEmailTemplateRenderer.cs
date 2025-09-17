using System.Text;
using System.Web;
using CMC.Application.Ports;

namespace CMC.Infrastructure.Services;

public sealed class BasicEmailTemplateRenderer : IEmailTemplateRenderer
{
    public Task<(string Subject, string Html, string? Text)> RenderAsync<T>(string templateKey, T model, string? culture = null, CancellationToken ct = default)
    {
        switch ((templateKey ?? "").Trim().ToLowerInvariant())
        {
            case "passwordreset":
            case "password-reset":
            case "reset":
                return Task.FromResult(RenderPasswordReset(model));

            case "welcome":
                return Task.FromResult(RenderWelcome(model));

            default:
                return Task.FromResult(($"CMC Notification", "<p>Hello from CMC.</p>", "Hello from CMC."));
        }
    }

    private static (string Subject, string Html, string? Text) RenderPasswordReset<T>(T model)
    {
        var email = Get(model, "Email");
        var token = Get(model, "Token");
        var link  = Get(model, "Link"); // optional vorgebauter Link

        var subject = "CMC – Password Reset";
        var url = string.IsNullOrWhiteSpace(link)
            ? $"/reset-password?token={HttpUtility.UrlEncode(token)}"
            : link!;

        var html = new StringBuilder()
            .Append("<p>Hello,</p>")
            .Append("<p>You requested a password reset for your CMC account.</p>")
            .Append($"<p><a href=\"{HttpUtility.HtmlAttributeEncode(url)}\">Reset your password</a></p>")
            .Append("<p>If you did not request this, you can ignore this email.</p>")
            .ToString();

        var text = $"Hello,\n\nUse the following link to reset your password:\n{url}\n\nIf you didn't request this, you can ignore this email.";

        return (subject, html, text);
    }

    private static (string Subject, string Html, string? Text) RenderWelcome<T>(T model)
    {
        var firstName = Get(model, "FirstName");
        var subject = "Welcome to CMC";
        var html = $"<p>Hi {(firstName ?? "there")},</p><p>Welcome to CMC! 🎉</p>";
        var text = $"Hi {(firstName ?? "there")},\n\nWelcome to CMC!";

        return (subject, html, text);
    }

    private static string? Get<T>(T obj, string prop)
        => obj?.GetType().GetProperty(prop)?.GetValue(obj)?.ToString();
}
