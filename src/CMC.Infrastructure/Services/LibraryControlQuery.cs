using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Services;

public sealed class LibraryControlQuery
{
    private readonly AppDbContext _db;

    public LibraryControlQuery(AppDbContext db) => _db = db;

    public async Task<HashSet<Guid>> GetIdsByLibraryScenarioIdsAsync(
        IEnumerable<Guid> libraryScenarioIds,
        CancellationToken ct = default)
    {
        var scen = (libraryScenarioIds ?? Array.Empty<Guid>()).Where(x => x != Guid.Empty).ToArray();
        if (scen.Length == 0) return new();

        var q = from link in _db.LibraryControlScenarios
                where scen.Contains(link.LibraryScenarioId)
                select link.LibraryControlId;

        return (await q.Distinct().ToListAsync(ct)).ToHashSet();
    }
}
