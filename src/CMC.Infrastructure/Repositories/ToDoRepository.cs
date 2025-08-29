using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories;

public class ToDoRepository : IToDoRepository
{
    private readonly AppDbContext _db;
    public ToDoRepository(AppDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

    public Task<ToDo?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.ToDos.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<ToDo>> GetAllAsync(CancellationToken ct = default)
        => _db.ToDos.AsNoTracking().OrderBy(x => x.CreatedAt).ToListAsync(ct);

    public async Task AddAsync(ToDo e, CancellationToken ct = default)
    {
        _db.ToDos.Add(e);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ToDo e, CancellationToken ct = default)
    {
        _db.ToDos.Update(e);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(ToDo e, CancellationToken ct = default)
    {
        _db.ToDos.Remove(e);
        await _db.SaveChangesAsync(ct);
    }

    public Task<List<ToDo>> GetByControlIdAsync(Guid controlId, CancellationToken ct = default)
        => _db.ToDos.AsNoTracking()
            .Where(x => x.ControlId == controlId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<ToDo> items, CancellationToken ct = default)
    {
        _db.ToDos.AddRange(items);
        await _db.SaveChangesAsync(ct);
    }
}
