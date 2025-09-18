using System.Threading;
using System.Threading.Tasks;

namespace CMC.Application.Ports;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string text, string links);
}
