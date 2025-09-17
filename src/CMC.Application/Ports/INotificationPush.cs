using CMC.Domain.Entities;

namespace CMC.Application.Ports;

/// Outbound-Port für Live-Push (Implementierung im Web-Layer, z.B. SignalR)
public interface INotificationPush
{
    Task NotifyCreated(Notification notification, CancellationToken ct = default);
    Task NotifyRead(Guid userId, Guid notificationId, CancellationToken ct = default);
}
