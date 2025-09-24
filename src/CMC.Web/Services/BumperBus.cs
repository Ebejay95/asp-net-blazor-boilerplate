using System;

namespace CMC.Web.Services;

public interface IBumperBus
{
    event Action<Bump>? Pushed;
    void Publish(string title, string message, string severity = "notice", int durationMs = 16000);
}

public sealed record Bump(string Title, string Message, string Severity, DateTimeOffset Until);

public sealed class BumperBus : IBumperBus
{
    public event Action<Bump>? Pushed;

    public void Publish(string title, string message, string severity = "notice", int durationMs = 16000)
    {
        var sev = Normalize(severity);
        var bump = new Bump(title, message, sev, DateTimeOffset.UtcNow.AddMilliseconds(durationMs));
        Pushed?.Invoke(bump);
    }

    private static string Normalize(string s) => (s ?? "").Trim().ToLowerInvariant() switch
    {
        "error" or "danger" or "err" => "error",
        "success" or "ok"            => "success",
        _                            => "notice"
    };
}
