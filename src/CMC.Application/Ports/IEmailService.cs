// CMC.Application.Ports/IEmailService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using CMC.Application.Ports.Mail;

namespace CMC.Application.Ports;

public interface IEmailService
{
    Task SendEmailAsync(
        string email,
        string subject,
        string text,
        IReadOnlyList<EmailButton> buttons,
        string? baseUrl = null
    );
}
