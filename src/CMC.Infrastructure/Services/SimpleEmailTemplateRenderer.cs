using CMC.Application.Ports;

public sealed class SimpleEmailTemplateRenderer : IEmailTemplateRenderer
{
    public Task<(string Subject, string Html, string? Text)>
        RenderAsync<TModel>(string templateKey, TModel model, string? culture = null, CancellationToken ct = default)
    {
        return templateKey switch
        {
            "PasswordReset" => Task.FromResult(PasswordReset(model!)),
            "Welcome"       => Task.FromResult(Welcome(model!)),
            _               => Task.FromResult(("CMC Notification", "<p>Unsupported template.</p>", "Unsupported template."))
        };

        static (string,string,string) PasswordReset(object m)
        {
            dynamic d = m!;
            var subject = "Reset your CMC password";
            var html = $@"
<p>Hi {(d.FirstName as string) ?? "there"},</p>
<p>We received a request to reset your password. Click the button below:</p>
<p><a href=""{d.Link}"" style=""display:inline-block;padding:10px 16px;background:#2563eb;color:#fff;border-radius:6px;text-decoration:none"">Reset Password</a></p>
<p>If the button doesn't work, open this link:<br/><code>{d.Link}</code></p>
<p>This link expires in 1 hour. If you didn’t request this, you can ignore this email.</p>";
            var text = $@"Hi {(d.FirstName as string) ?? "there"},

We received a request to reset your password.
Reset link (valid 1 hour): {d.Link}

If you didn’t request this, ignore this email.";
            return (subject, html, text);
        }

        static (string,string,string) Welcome(object m)
        {
            dynamic d = m!;
            var subject = "Welcome to CMC 🎉";
            var html = $@"<p>Hi {d.FirstName},</p><p>Welcome aboard! You can now log in to CMC.</p>";
            var text = $"Hi {d.FirstName},\n\nWelcome aboard! You can now log in to CMC.";
            return (subject, html, text);
        }
    }
}
