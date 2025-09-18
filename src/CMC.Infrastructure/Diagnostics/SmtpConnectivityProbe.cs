using System.Net.Sockets;
using CMC.Application.Ports.Mail;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CMC.Infrastructure.Diagnostics;

// Startet beim Boot & prüft DNS + TCP + optional STARTTLS Banner (ohne Auth)
public sealed class SmtpConnectivityProbe : IHostedService
{
    private readonly ILogger<SmtpConnectivityProbe> _log;
    private readonly MailOptions _opts;
    private readonly bool _enabled;

    public SmtpConnectivityProbe(IOptions<MailOptions> opts, ILogger<SmtpConnectivityProbe> log, IConfiguration cfg)
    {
        _opts = opts.Value;
        _log = log;
        _enabled = string.Equals(cfg["MAIL_DIAG"], "true", StringComparison.OrdinalIgnoreCase);
    }

    public async Task StartAsync(CancellationToken ct)
    {
        if (!_enabled) return;

        try
        {
            _log.LogInformation("🔎 SMTP diag enabled. host={Host} port={Port} starttls={Tls}", _opts.Smtp.Host, _opts.Smtp.Port, _opts.Smtp.UseStartTls);

            var addresses = await System.Net.Dns.GetHostAddressesAsync(_opts.Smtp.Host);
            foreach (var ip in addresses)
                _log.LogInformation("DNS {Host} -> {IP}", _opts.Smtp.Host, ip);

            using var tcp = new TcpClient();
            var connectCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_opts.Smtp.TimeoutMs));
            await tcp.ConnectAsync(_opts.Smtp.Host, _opts.Smtp.Port, connectCts.Token);
            _log.LogInformation("TCP connect ok to {Host}:{Port}", _opts.Smtp.Host, _opts.Smtp.Port);

            using var stream = tcp.GetStream();
            stream.ReadTimeout = _opts.Smtp.TimeoutMs;
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream) { NewLine = "\r\n", AutoFlush = true };

            // SMTP Banner lesen
            var banner = await reader.ReadLineAsync();
            _log.LogInformation("SMTP banner: {Banner}", banner);

            // EHLO
            await writer.WriteLineAsync($"EHLO cmc.local");
            for (int i = 0; i < 10; i++)
            {
                var line = await reader.ReadLineAsync();
                if (line == null || !line.StartsWith("250-") && !line.StartsWith("250 ")) break;
                _log.LogDebug("EHLO: {Line}", line);
                if (line.StartsWith("250 ")) break;
            }

            if (_opts.Smtp.UseStartTls)
            {
                await writer.WriteLineAsync("STARTTLS");
                var resp = await reader.ReadLineAsync();
                _log.LogInformation("STARTTLS resp: {Resp}", resp);
                // Wir verifizieren nur, dass der Server STARTTLS anbietet/annimmt. Handshake selbst überlassen wir SmtpClient.
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "❌ SMTP connectivity probe failed");
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
