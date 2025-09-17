using CMC.Application.Ports;
using CMC.Domain.Entities;

namespace CMC.Application.Services;

public class NotificationService
{
    private readonly INotificationRepository _repo;
    private readonly INotificationPush? _push; // optionaler Outbound-Port (z.B. SignalR)

    public NotificationService(INotificationRepository repo, INotificationPush? push = null)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _push = push; // darf null sein
    }

    public Task<List<Notification>> ListAsync(Guid userId, int take = 100, CancellationToken ct = default)
        => _repo.GetForUserAsync(userId, take, ct);

    public Task<List<Notification>> ListUnreadAsync(Guid userId, int take = 50, CancellationToken ct = default)
        => _repo.GetUnreadForUserAsync(userId, take, ct);

    public Task<int> CountUnreadAsync(Guid userId, CancellationToken ct = default)
        => _repo.CountUnreadAsync(userId, ct);

    public async Task<Notification> CreateAsync(
        Guid userId,
        string title,
        string message,
        string severity,
        Guid? customerId = null,
        CancellationToken ct = default)
    {
        var sev = NormalizeSeverity(severity);
        var n = new Notification(userId, title, message, sev, customerId);
        await _repo.AddAsync(n, ct);

        if (_push is not null)
            await _push.NotifyCreated(n, ct);

        return n;
    }

    public async Task<bool> MarkReadAsync(Guid id, CancellationToken ct = default)
    {
        var n = await _repo.GetByIdAsync(id, ct);
        if (n is null) return false;

        n.MarkRead();
        await _repo.SaveAsync(ct);

        if (_push is not null)
            await _push.NotifyRead(n.UserId, n.Id, ct);

        return true;
    }

    private static string NormalizeSeverity(string s)
        => s?.Trim().ToLowerInvariant() switch
        {
            "error" or "danger" or "err" => "error",
            "success" or "ok"            => "success",
            _                            => "notice"
        };
}
