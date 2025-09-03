using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Services;

public sealed class LibraryScenarioQuery
{
    private readonly AppDbContext _db;

    public LibraryScenarioQuery(AppDbContext db) => _db = db;

    public sealed record LibraryScenarioLite(Guid Id, string Name);

    public async Task<List<LibraryScenarioLite>> GetByIndustriesAsync(
        IEnumerable<Guid> industryIds,
        CancellationToken ct = default)
    {
        var filter = (industryIds ?? Array.Empty<Guid>()).Where(x => x != Guid.Empty).ToHashSet();

        if (filter.Count == 0)
        {
            return await _db.LibraryScenarios
                .Where(ls => !ls.IsDeleted)
                .OrderBy(ls => ls.Name)
                .Select(ls => new LibraryScenarioLite(ls.Id, ls.Name))
                .ToListAsync(ct);
        }

        return await _db.LibraryScenarios
            .Where(ls => !ls.IsDeleted)
            .Where(ls => _db.LibraryScenarioIndustries.Any(li => li.LibraryScenarioId == ls.Id && filter.Contains(li.IndustryId)))
            .OrderBy(ls => ls.Name)
            .Select(ls => new LibraryScenarioLite(ls.Id, ls.Name))
            .ToListAsync(ct);
    }
}
