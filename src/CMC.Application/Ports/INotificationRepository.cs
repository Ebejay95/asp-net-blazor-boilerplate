using CMC.Domain.Entities;

namespace CMC.Application.Ports;

public interface INotificationRepository
{
    Task AddAsync(Notification n, CancellationToken ct = default);
    Task<List<Notification>> GetForUserAsync(Guid userId, int take = 100, CancellationToken ct = default);
    Task<List<Notification>> GetUnreadForUserAsync(Guid userId, int take = 50, CancellationToken ct = default);
    Task<int> CountUnreadAsync(Guid userId, CancellationToken ct = default);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}
