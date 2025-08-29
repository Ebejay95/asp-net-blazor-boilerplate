using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories;

public class RiskAcceptanceRepository : IRiskAcceptanceRepository
{
    private readonly AppDbContext _db;
    public RiskAcceptanceRepository(AppDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

    public Task<RiskAcceptance?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.RiskAcceptances.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<RiskAcceptance>> GetAllAsync(CancellationToken ct = default)
        => _db.RiskAcceptances.AsNoTracking().OrderByDescending(x => x.ExpiresAt).ToListAsync(ct);

    public async Task AddAsync(RiskAcceptance e, CancellationToken ct = default)
    {
        _db.RiskAcceptances.Add(e);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RiskAcceptance e, CancellationToken ct = default)
    {
        _db.RiskAcceptances.Update(e);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(RiskAcceptance e, CancellationToken ct = default)
    {
        _db.RiskAcceptances.Remove(e);
        await _db.SaveChangesAsync(ct);
    }

    public Task<List<RiskAcceptance>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
        => _db.RiskAcceptances.AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.ExpiresAt)
            .ToListAsync(ct);

    public Task<List<RiskAcceptance>> GetByControlAsync(Guid controlId, CancellationToken ct = default)
        => _db.RiskAcceptances.AsNoTracking()
            .Where(x => x.ControlId == controlId)
            .OrderByDescending(x => x.ExpiresAt)
            .ToListAsync(ct);

    public Task<List<RiskAcceptance>> GetActiveByControlAsync(Guid controlId, DateTimeOffset asOf, CancellationToken ct = default)
        => _db.RiskAcceptances.AsNoTracking()
            .Where(x => x.ControlId == controlId && x.ExpiresAt >= asOf)
            .OrderByDescending(x => x.ExpiresAt)
            .ToListAsync(ct);
}
