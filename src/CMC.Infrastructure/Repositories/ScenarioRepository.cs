using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories;

public class ScenarioRepository : IScenarioRepository
{
    private readonly AppDbContext _db;
    public ScenarioRepository(AppDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

    // Für Detail-View/Bearbeiten: alles mitladen (inkl. TagLinks für korrektes Diffing in SetTags)
    public Task<Scenario?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Scenarios
            .Include(s => s.Customer)
            .Include(s => s.LibraryScenario)
            .Include(s => s.TagLinks)
                .ThenInclude(st => st.Tag) // Labels + Diff
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    // Fürs Grid: Tag-Namen anzeigen -> TagLinks.Tag inkludieren
    public Task<List<Scenario>> GetAllAsync(CancellationToken ct = default)
        => _db.Scenarios
            .Include(s => s.Customer)
            .Include(s => s.LibraryScenario)
            .Include(s => s.TagLinks)
                .ThenInclude(st => st.Tag) // <— WICHTIG für TagLabels im Grid
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

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
        if (e == null) throw new ArgumentNullException(nameof(e));
        e.Delete();
        _db.Scenarios.Update(e);
        await _db.SaveChangesAsync(ct);
    }

    // Auch hier für Grid-/Listenansicht die Tag-Namen laden
    public Task<List<Scenario>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
        => _db.Scenarios
            .Include(s => s.Customer)
            .Include(s => s.LibraryScenario)
            .Include(s => s.TagLinks)
                .ThenInclude(st => st.Tag) // <— WICHTIG
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<Scenario> items, CancellationToken ct = default)
    {
        _db.Scenarios.AddRange(items);
        await _db.SaveChangesAsync(ct);
    }
	public Task<int> CountByCustomerAsync(Guid customerId, CancellationToken ct = default)
    => _db.Scenarios.CountAsync(s => s.CustomerId == customerId && !s.IsDeleted, ct);
}
