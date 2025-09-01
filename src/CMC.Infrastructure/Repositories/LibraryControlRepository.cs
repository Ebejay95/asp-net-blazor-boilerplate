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
    public class LibraryControlRepository : ILibraryControlRepository
    {
        private readonly AppDbContext _db;
        public LibraryControlRepository(AppDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public Task<LibraryControl?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _db.LibraryControls
                  .Include(lc => lc.TagLinks).ThenInclude(t => t.Tag)
                  .Include(lc => lc.IndustryLinks).ThenInclude(i => i.Industry)
                  .FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<List<LibraryControl>> GetAllAsync(CancellationToken ct = default)
            => _db.LibraryControls
                  .Include(lc => lc.TagLinks).ThenInclude(t => t.Tag)
                  .Include(lc => lc.IndustryLinks).ThenInclude(i => i.Industry)
                  .AsNoTracking()
                  .OrderBy(x => x.Name)
                  .ToListAsync(ct);

        public async Task AddAsync(LibraryControl e, CancellationToken ct = default)
        {
            _db.LibraryControls.Add(e);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(LibraryControl e, CancellationToken ct = default)
        {
            _db.LibraryControls.Update(e);
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(LibraryControl e, CancellationToken ct = default)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            e.Delete();
            _db.LibraryControls.Update(e);
            await _db.SaveChangesAsync(ct);
        }

        public Task<List<LibraryControl>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            var list = (ids ?? Enumerable.Empty<Guid>()).ToList();
            return _db.LibraryControls.AsNoTracking()
                .Where(x => list.Contains(x.Id))
                .OrderBy(x => x.Name)
                .ToListAsync(ct);
        }
    }
}
