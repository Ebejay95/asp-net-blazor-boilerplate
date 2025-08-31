using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories
{
	public class LibraryScenarioRepository : ILibraryScenarioRepository
	{
		private readonly AppDbContext _db;
		public LibraryScenarioRepository(AppDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

		public Task<LibraryScenario?> GetByIdAsync(Guid id, CancellationToken ct = default)
			=> _db.LibraryScenarios.FirstOrDefaultAsync(x => x.Id == id, ct);

		public Task<List<LibraryScenario>> GetAllAsync(CancellationToken ct = default)
			=> _db.LibraryScenarios.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

		public async Task AddAsync(LibraryScenario e, CancellationToken ct = default)
		{
			_db.LibraryScenarios.Add(e);
			await _db.SaveChangesAsync(ct);
		}

		public async Task UpdateAsync(LibraryScenario e, CancellationToken ct = default)
		{
			_db.LibraryScenarios.Update(e);
			await _db.SaveChangesAsync(ct);
		}

		public async Task DeleteAsync(LibraryScenario e, CancellationToken ct = default)
		{
			if (e == null) throw new ArgumentNullException(nameof(e));
			e.Delete();
			_db.LibraryScenarios.Update(e);
			await _db.SaveChangesAsync(ct);
		}

		public Task<List<LibraryScenario>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
		{
			var list = (ids ?? Enumerable.Empty<Guid>()).ToList();
			return _db.LibraryScenarios.AsNoTracking()
				.Where(x => list.Contains(x.Id))
				.OrderBy(x => x.Name)
				.ToListAsync(ct);
		}
	}
}
