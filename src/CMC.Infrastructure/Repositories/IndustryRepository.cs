using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories
{
	public class IndustryRepository : IIndustryRepository
	{
		private readonly AppDbContext _db;
		public IndustryRepository(AppDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

		public Task<Industry?> GetByIdAsync(Guid id, CancellationToken ct = default)
			=> _db.Industries.FirstOrDefaultAsync(x => x.Id == id, ct);

		public Task<List<Industry>> GetAllAsync(CancellationToken ct = default)
			=> _db.Industries.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

		public async Task AddAsync(Industry e, CancellationToken ct = default)
		{
			_db.Industries.Add(e);
			await _db.SaveChangesAsync(ct);
		}

		public async Task UpdateAsync(Industry e, CancellationToken ct = default)
		{
			_db.Industries.Update(e);
			await _db.SaveChangesAsync(ct);
		}

		public async Task DeleteAsync(Industry e, CancellationToken ct = default)
		{
			_db.Industries.Remove(e);
			await _db.SaveChangesAsync(ct);
		}

		public Task<Industry?> GetByNameAsync(string name, CancellationToken ct = default)
			=> _db.Industries.AsNoTracking().FirstOrDefaultAsync(x => x.Name == name, ct);

		public Task<bool> ExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
			var q = _db.Industries.AsQueryable().Where(x => x.Name == name);
			if (excludeId.HasValue) q = q.Where(x => x.Id != excludeId.Value);
			return q.AnyAsync(ct);
		}

		public Task<List<Industry>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
		{
			var set = ids?.Where(x => x != Guid.Empty).Distinct().ToArray() ?? Array.Empty<Guid>();
			return _db.Industries.Where(x => set.Contains(x.Id)).ToListAsync(ct);
		}
	}
}
