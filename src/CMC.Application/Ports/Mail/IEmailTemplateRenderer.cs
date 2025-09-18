using System.Threading;
using System.Threading.Tasks;

namespace CMC.Application.Ports.Mail;

public interface IEmailTemplateRenderer
{
    /// <summary>
    /// Rendert Templates und liefert (Subject, HtmlBody, TextBody).
    /// </summary>
    Task<(string Subject, string Html, string? Text)> RenderEmailAsync(
        string subject,
        string text,
        object links
    );
}
