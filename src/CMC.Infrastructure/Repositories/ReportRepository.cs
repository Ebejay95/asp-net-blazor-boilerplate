using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
	private readonly AppDbContext _db;
	public ReportRepository(AppDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

	public Task<Report?> GetByIdAsync(Guid id, CancellationToken ct = default)
		=> _db.Reports.FirstOrDefaultAsync(x => x.Id == id, ct);

	public Task<List<Report>> GetAllAsync(CancellationToken ct = default)
		=> _db.Reports.AsNoTracking().OrderByDescending(x => x.GeneratedAt).ToListAsync(ct);

	public async Task AddAsync(Report e, CancellationToken ct = default)
	{
		_db.Reports.Add(e);
		await _db.SaveChangesAsync(ct);
	}

	public async Task UpdateAsync(Report e, CancellationToken ct = default)
	{
		_db.Reports.Update(e);
		await _db.SaveChangesAsync(ct);
	}

	public async Task DeleteAsync(Report e, CancellationToken ct = default)
	{
		_db.Reports.Remove(e);
		await _db.SaveChangesAsync(ct);
	}

	public Task<List<Report>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
		=> _db.Reports.AsNoTracking()
			.Where(x => x.CustomerId == customerId)
			.OrderByDescending(x => x.GeneratedAt)
			.ToListAsync(ct);
}
