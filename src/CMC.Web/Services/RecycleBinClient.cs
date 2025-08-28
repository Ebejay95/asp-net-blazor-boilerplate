using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Contracts.RecycleBin;
using CMC.Infrastructure.Services;

namespace CMC.Web.Services;

public sealed class RecycleBinClient : IRecycleBinClient
{
    private readonly RecycleBinService _service;

    public RecycleBinClient(RecycleBinService service)
    {
        _service = service;
    }

    public async Task<List<RecycleBinItemDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _service.GetAllAsync(ct);
        return items.Select(ToDto).ToList();
    }

    public Task RestoreAsync(RecycleBinItemDto item, CancellationToken ct = default)
        // Hinweis: Der Service hat (table, assetId, userEmail?, ct). userEmail wird hier bewusst übersprungen.
        => _service.RestoreAsync(item.Table, item.AssetId, ct: ct);

    public Task PurgeAsync(RecycleBinItemDto item, CancellationToken ct = default)
        => _service.PurgeAsync(item.Table, item.AssetId, ct: ct);

    private static RecycleBinItemDto ToDto(RecycleBinItem x)
    {
        // Annahmen zur neuen DTO-Konstruktor-Signatur:
        // RecycleBinItemDto(string table, Guid assetId, string title, string? subtitle, string? extra, DateTimeOffset deletedAt, string? deletedBy)
        var title = x.Display;              // früher: Display -> jetzt: Title
        var subtitle = x.EntityType;        // optional als Untertitel anzeigen
        var extra = (string?)null;          // falls es einen zusätzlichen optionalen String gibt

        var deletedAt = x.DeletedAt ?? DateTimeOffset.UtcNow; // Non-null erwartet
        var deletedBy = x.DeletedBy;

        return new RecycleBinItemDto(
            x.Table,
            x.AssetId,
            title,
            subtitle,
            deletedBy,
            deletedAt,
            extra
        );
    }
}
