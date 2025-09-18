using System.Threading;
using System.Threading.Tasks;

namespace CMC.Application.Ports.Mail
{
    public interface IEmailTemplateRenderer
    {
        Task<(string Subject, string Html, string? Text)> RenderAsync(
            string templateName, object model, CancellationToken ct = default);
    }
}
