using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMC.Infrastructure.Services;

namespace CMC.Web.Services;

/// <summary>
/// Blazor-Server: nutzt direkt den EF-basierten RevisionService aus Infrastructure.
/// Keine Entity-spezifische Logik, nur (table, id).
/// </summary>
public sealed class EfRevisionsClient : IRevisionsClient
{
    private readonly RevisionService _svc;

    public EfRevisionsClient(RevisionService svc) => _svc = svc;

    public async Task<List<EditRevisionItem>> GetAsync(string table, Guid assetId, int take = 100)
    {
        var list = await _svc.GetByAssetAsync(table, assetId, take);
        return list.Select(r => new EditRevisionItem(
            Id: r.Id,
            CreatedAt: r.CreatedAt,
            Action: r.Action,
            UserEmail: r.UserEmail,
            Data: r.Data
        )).ToList();
    }

    public Task RestoreAsync(long revisionId) => _svc.RestoreAsync(revisionId);
}
