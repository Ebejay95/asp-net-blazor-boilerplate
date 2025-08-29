using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories;

public class EvidenceRepository : IEvidenceRepository
{
	private readonly AppDbContext _db;
	public EvidenceRepository(AppDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

	public Task<Evidence?> GetByIdAsync(Guid id, CancellationToken ct = default)
		=> _db.Evidence.FirstOrDefaultAsync(x => x.Id == id, ct);

	public Task<List<Evidence>> GetAllAsync(CancellationToken ct = default)
		=> _db.Evidence.AsNoTracking().OrderByDescending(x => x.CollectedAt).ToListAsync(ct);

	public async Task AddAsync(Evidence e, CancellationToken ct = default)
	{
		_db.Evidence.Add(e);
		await _db.SaveChangesAsync(ct);
	}

	public async Task UpdateAsync(Evidence e, CancellationToken ct = default)
	{
		_db.Evidence.Update(e);
		await _db.SaveChangesAsync(ct);
	}

	public async Task DeleteAsync(Evidence e, CancellationToken ct = default)
	{
		_db.Evidence.Remove(e);
		await _db.SaveChangesAsync(ct);
	}

	public Task<List<Evidence>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
		=> _db.Evidence.AsNoTracking()
			.Where(x => x.CustomerId == customerId)
			.OrderByDescending(x => x.CollectedAt)
			.ToListAsync(ct);
}
