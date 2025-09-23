using CMC.Application.Ports;
using CMC.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using CMC.Web.Hubs;

namespace CMC.Web.Services;

public sealed class SignalRNotificationPush : INotificationPush
{
    private readonly IHubContext<NotificationHub> _hub;

    public SignalRNotificationPush(IHubContext<NotificationHub> hub) => _hub = hub;

    public Task NotifyCreated(Notification n, CancellationToken ct = default)
        => _hub.Clients.User(n.UserId.ToString()).SendAsync("notificationCreated", new
        {
            n.Id,
            n.Title,
            n.Message,
            Severity = n.Severity,
            n.Status,
            n.CreatedAt
        }, ct);

    public Task NotifyRead(Guid userId, Guid notificationId, CancellationToken ct = default)
        => _hub.Clients.User(userId.ToString()).SendAsync("notificationRead", new { Id = notificationId }, ct);
}
