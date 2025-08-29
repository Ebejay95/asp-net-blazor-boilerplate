using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories;

public class ScenarioRepository : IScenarioRepository
{
	private readonly AppDbContext _db;
	public ScenarioRepository(AppDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

	public Task<Scenario?> GetByIdAsync(Guid id, CancellationToken ct = default)
		=> _db.Scenarios.FirstOrDefaultAsync(x => x.Id == id, ct);

	public Task<List<Scenario>> GetAllAsync(CancellationToken ct = default)
		=> _db.Scenarios.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

	public async Task AddAsync(Scenario e, CancellationToken ct = default)
	{
		_db.Scenarios.Add(e);
		await _db.SaveChangesAsync(ct);
	}

	public async Task UpdateAsync(Scenario e, CancellationToken ct = default)
	{
		_db.Scenarios.Update(e);
		await _db.SaveChangesAsync(ct);
	}

	public async Task DeleteAsync(Scenario e, CancellationToken ct = default)
	{
		_db.Scenarios.Remove(e);
		await _db.SaveChangesAsync(ct);
	}

	public Task<List<Scenario>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
		=> _db.Scenarios.AsNoTracking()
			.Where(x => x.CustomerId == customerId)
			.OrderBy(x => x.Name)
			.ToListAsync(ct);

	public async Task AddRangeAsync(IEnumerable<Scenario> items, CancellationToken ct = default)
	{
		_db.Scenarios.AddRange(items);
		await _db.SaveChangesAsync(ct);
	}
}
