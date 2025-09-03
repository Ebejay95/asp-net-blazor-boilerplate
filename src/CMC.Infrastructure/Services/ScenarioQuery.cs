using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Services;

public sealed class ScenarioQuery
{
    private readonly AppDbContext _db;
    public ScenarioQuery(AppDbContext db) => _db = db;

    public Task<int> CountByCustomerAsync(Guid customerId, CancellationToken ct = default)
        => _db.Scenarios.CountAsync(s => s.CustomerId == customerId && !s.IsDeleted, ct);

    public async Task<List<(Guid Id, string Name)>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
        => await _db.Scenarios
            .Where(s => s.CustomerId == customerId && !s.IsDeleted)
            .OrderBy(s => s.Name)
            .Select(s => new ValueTuple<Guid,string>(s.Id, s.Name))
            .ToListAsync(ct);
}
