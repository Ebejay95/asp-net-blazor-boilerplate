// CMC.Application.Ports.Mail/IEmailTemplateRenderer.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMC.Application.Ports.Mail;

public interface IEmailTemplateRenderer
{
    Task<(string Subject, string Html, string? Text)> RenderEmailAsync(
        string subject,
        string text,
        IReadOnlyList<EmailButton> buttons,
        string? baseUrl = null
    );
}
