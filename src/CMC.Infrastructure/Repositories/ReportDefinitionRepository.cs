using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories;

public class ReportDefinitionRepository : IReportDefinitionRepository
{
	private readonly AppDbContext _db;
	public ReportDefinitionRepository(AppDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

	public Task<ReportDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default)
		=> _db.ReportDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct);

	public Task<List<ReportDefinition>> GetAllAsync(CancellationToken ct = default)
		=> _db.ReportDefinitions.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

	public async Task AddAsync(ReportDefinition e, CancellationToken ct = default)
	{
		_db.ReportDefinitions.Add(e);
		await _db.SaveChangesAsync(ct);
	}

	public async Task UpdateAsync(ReportDefinition e, CancellationToken ct = default)
	{
		_db.ReportDefinitions.Update(e);
		await _db.SaveChangesAsync(ct);
	}

	public async Task DeleteAsync(ReportDefinition e, CancellationToken ct = default)
	{
		_db.ReportDefinitions.Remove(e);
		await _db.SaveChangesAsync(ct);
	}

	public Task<List<ReportDefinition>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
		=> _db.ReportDefinitions.AsNoTracking()
			.Where(x => x.CustomerId == customerId)
			.OrderBy(x => x.Name)
			.ToListAsync(ct);
}
