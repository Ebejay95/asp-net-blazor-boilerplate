using System.Threading;
using System.Threading.Tasks;

namespace CMC.Application.Ports;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default);
}
