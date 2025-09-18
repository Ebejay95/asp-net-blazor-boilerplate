using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CMC.Infrastructure.Diagnostics;

/// <summary>
/// Einfache SMTP-Konnektivitätsprobe ohne DI-Scope/HostedService.
/// Lies Host/Port aus der Konfiguration und logge Ergebnis.
/// </summary>
public static class SmtpConnectivityProbe
{
    public static async Task RunAsync(IConfiguration cfg, ILogger? logger = null, CancellationToken ct = default)
    {
        var host   = cfg["Mail:Smtp:Host"];
        var portOk = int.TryParse(cfg["Mail:Smtp:Port"], out var port);

        if (string.IsNullOrWhiteSpace(host) || !portOk)
        {
            logger?.LogWarning("SMTP probe: skipped (Mail:Smtp:Host/Port not configured).");
            return;
        }

        logger?.LogInformation("SMTP probe: trying {Host}:{Port} ...", host, port);

        try
        {
            using var client = new TcpClient
            {
                ReceiveTimeout = 5000,
                SendTimeout    = 5000
            };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            await client.ConnectAsync(host, port, cts.Token);

            using var stream = client.GetStream();
            var buffer = new byte[512];
            var read   = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cts.Token);
            var banner = read > 0 ? System.Text.Encoding.ASCII.GetString(buffer, 0, read).Trim() : "<no banner>";

            logger?.LogInformation("SMTP probe: connected. Banner: {Banner}", banner);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "SMTP probe: connection to {Host}:{Port} failed", host, port);
        }
    }
}
