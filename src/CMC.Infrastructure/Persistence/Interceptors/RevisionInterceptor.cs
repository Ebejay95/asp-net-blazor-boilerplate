using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using CMC.Domain.Entities;

namespace CMC.Infrastructure.Persistence.Interceptors;

public sealed class RevisionInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOpts =
        new(JsonSerializerDefaults.Web) { WriteIndented = false };

    private readonly IHttpContextAccessor _http;

    public RevisionInterceptor(IHttpContextAccessor http) => _http = http;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var ctx = eventData.Context!;
        var now = DateTimeOffset.UtcNow;
        var email = GetUserEmail();

        foreach (var e in ctx.ChangeTracker.Entries().ToList())
        {
            // 1) Soft-Delete für alle ISoftDeletable
            if (e.State == EntityState.Deleted && e.Entity is ISoftDeletable sd)
            {
                e.State = EntityState.Modified;
                sd.IsDeleted = true;
                sd.DeletedAt = now;
                sd.DeletedBy = email;
            }

            // 2) Revisions nur für IVersionedEntity (wie bisher)
            if (e.Entity is not IVersionedEntity) continue;

            var action = e.State switch
            {
                EntityState.Added    => "Create",
                EntityState.Modified => "Update",
                EntityState.Deleted  => "Delete",
                _                    => null
            };
            if (action is null) continue;

            var table = GetTableFullName(e);
            var id = GetGuidKey(e);
            var snapshot = BuildSnapshotJson(e);

            ctx.Set<Revision>().Add(new Revision
            {
                Table = table,
                AssetId = id,
                Action = action,
                UserEmail = email,
                Data = snapshot,
                CreatedAt = now
            });
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private string? GetUserEmail()
    {
        var u = _http.HttpContext?.User;
        return u?.FindFirst(ClaimTypes.Email)?.Value
            ?? u?.Identity?.Name
            ?? u?.FindFirst("email")?.Value;
    }

    private static string BuildSnapshotJson(EntityEntry e)
    {
        // Nur gemappte Properties, keine Navigations-Props
        var dict = e.Properties
            .Where(p => !p.Metadata.IsShadowProperty())
            .ToDictionary(
                p => p.Metadata.Name,
                p => p.Metadata.IsPrimaryKey() || e.State != EntityState.Deleted
                        ? p.CurrentValue
                        : p.OriginalValue
            );

        return JsonSerializer.Serialize(dict, JsonOpts);
    }

    private static Guid GetGuidKey(EntityEntry e)
    {
        var pk = e.Properties.First(p => p.Metadata.IsPrimaryKey());
        var v = pk.CurrentValue ?? pk.OriginalValue;
        return v is Guid g ? g : Guid.Parse(v!.ToString()!);
    }

    private static string GetTableFullName(EntityEntry e)
    {
        var schema = e.Metadata.GetSchema();
        var name = e.Metadata.GetTableName();
        return string.IsNullOrWhiteSpace(schema) ? name! : $"{schema}.{name}";
    }
}
