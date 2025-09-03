using System;
using System.Threading;
using System.Threading.Tasks;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Services;

public sealed class ControlQuery
{
    private readonly AppDbContext _db;
    public ControlQuery(AppDbContext db) => _db = db;

    public Task<int> CountByCustomerAsync(Guid customerId, CancellationToken ct = default)
        => _db.Controls.CountAsync(c => c.CustomerId == customerId && !c.IsDeleted, ct);
}
