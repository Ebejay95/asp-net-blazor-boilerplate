using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _db;
    public NotificationRepository(AppDbContext db) => _db = db;

    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Notifications.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Notification n, CancellationToken ct = default)
    { _db.Notifications.Add(n); await _db.SaveChangesAsync(ct); }

    public Task<List<Notification>> GetForUserAsync(Guid userId, int take = 100, CancellationToken ct = default)
        => _db.Notifications.AsNoTracking().Where(x => x.UserId == userId)
              .OrderByDescending(x => x.CreatedAt).Take(take).ToListAsync(ct);

    public Task<List<Notification>> GetUnreadForUserAsync(Guid userId, int take = 50, CancellationToken ct = default)
        => _db.Notifications.AsNoTracking().Where(x => x.UserId == userId && x.Status == "unread")
              .OrderByDescending(x => x.CreatedAt).Take(take).ToListAsync(ct);

    public Task<int> CountUnreadAsync(Guid userId, CancellationToken ct = default)
        => _db.Notifications.Where(x => x.UserId == userId && x.Status == "unread").CountAsync(ct);

    public Task SaveAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
