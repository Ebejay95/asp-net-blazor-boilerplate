using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories
{
	public class TagRepository : ITagRepository
	{
		private readonly AppDbContext _db;
		public TagRepository(AppDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

		public Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct = default)
			=> _db.Tags.FirstOrDefaultAsync(x => x.Id == id, ct);

		public Task<List<Tag>> GetAllAsync(CancellationToken ct = default)
			=> _db.Tags.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

		public async Task AddAsync(Tag e, CancellationToken ct = default)
		{
			_db.Tags.Add(e);
			await _db.SaveChangesAsync(ct);
		}

		public async Task UpdateAsync(Tag e, CancellationToken ct = default)
		{
			_db.Tags.Update(e);
			await _db.SaveChangesAsync(ct);
		}

		public async Task DeleteAsync(Tag e, CancellationToken ct = default)
		{
			_db.Tags.Remove(e);
			await _db.SaveChangesAsync(ct);
		}

		public Task<Tag?> GetByNameAsync(string name, CancellationToken ct = default)
			=> _db.Tags.AsNoTracking().FirstOrDefaultAsync(x => x.Name == name, ct);

		public Task<bool> ExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
			var q = _db.Tags.AsQueryable().Where(x => x.Name == name);
			if (excludeId.HasValue) q = q.Where(x => x.Id != excludeId.Value);
			return q.AnyAsync(ct);
		}

		public Task<List<Tag>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
		{
			var set = ids?.Where(x => x != Guid.Empty).Distinct().ToArray() ?? Array.Empty<Guid>();
			return _db.Tags.Where(x => set.Contains(x.Id)).ToListAsync(ct);
		}
	}
}
