using CMC.Application.Ports;
using CMC.Contracts.Notifications;
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

    // CRUD: Create
    public async Task<NotificationDto> CreateAsync(
        Guid userId,
        string title,
        string message,
        string severity,
        Guid? customerId = null,
        CancellationToken ct = default)
    {
        var sev = NormalizeSeverity(severity);
        var notification = new Notification(userId, title, message, sev, customerId);

        await _repo.AddAsync(notification, ct);

        // Live-Push (falls implementiert)
        if (_push is not null)
            await _push.NotifyCreated(notification, ct);

        return MapToDto(notification);
    }

    // CRUD: Read (Single)
    public async Task<NotificationDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var notification = await _repo.GetByIdAsync(id, ct);
        return notification != null ? MapToDto(notification) : null;
    }

    // CRUD: Read (List for User)
    public async Task<List<NotificationDto>> ListAsync(Guid userId, int take = 100, CancellationToken ct = default)
    {
        var notifications = await _repo.GetForUserAsync(userId, take, ct);
        return notifications.Select(MapToDto).ToList();
    }

    // CRUD: Read (Unread for User)
    public async Task<List<NotificationDto>> ListUnreadAsync(Guid userId, int take = 50, CancellationToken ct = default)
    {
        var notifications = await _repo.GetUnreadForUserAsync(userId, take, ct);
        return notifications.Select(MapToDto).ToList();
    }

    // CRUD: Update (Mark as Read)
    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default)
    {
        var success = await _repo.AcknowledgeAsync(notificationId, userId, ct);

        // Live-Push für Read-Status (falls implementiert)
        if (success && _push is not null)
            await _push.NotifyRead(userId, notificationId, ct);

        return success;
    }

    // CRUD: Bulk Update (Mark multiple as Read)
    public async Task<int> MarkMultipleAsReadAsync(List<Guid> notificationIds, Guid userId, CancellationToken ct = default)
    {
        int count = 0;
        foreach (var id in notificationIds)
        {
            var success = await _repo.AcknowledgeAsync(id, userId, ct);
            if (success)
            {
                count++;
                // Live-Push für jeden gelesenen Status
                if (_push is not null)
                    await _push.NotifyRead(userId, id, ct);
            }
        }
        return count;
    }

    // CRUD: Delete (falls gewünscht - Soft Delete über Repository)
    public async Task<bool> DeleteAsync(Guid notificationId, Guid userId, CancellationToken ct = default)
    {
        // Implementation abhängig von Ihrem Soft-Delete-Pattern
        // Hier würden Sie _repo.DeleteAsync aufrufen, falls implementiert
        throw new NotImplementedException("Delete operation not yet implemented");
    }

    // Utility Methods
    public Task<int> CountUnreadAsync(Guid userId, CancellationToken ct = default)
        => _repo.CountUnreadAsync(userId, ct);

    private static string NormalizeSeverity(string s)
        => s?.Trim().ToLowerInvariant() switch
        {
            "error" or "danger" or "err" => "error",
            "success" or "ok" => "success",
            "warning" or "warn" => "warning",
            _ => "notice"
        };

    private static NotificationDto MapToDto(Notification notification)
        => new(
            Id: notification.Id,
            Title: notification.Title,
            Message: notification.Message,
            Severity: notification.Severity,
            CreatedAt: notification.CreatedAt,
            IsRead: notification.Status == "read"
        );
}
