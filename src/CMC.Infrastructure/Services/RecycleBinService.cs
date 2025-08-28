// src/CMC.Infrastructure/Services/RecycleBinService.cs
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CMC.Infrastructure.Services;

public sealed class RecycleBinService
{
    private readonly AppDbContext _db;
    private readonly RevisionService _revisions;

    public RecycleBinService(AppDbContext db, RevisionService revisions)
    {
        _db = db;
        _revisions = revisions;
    }

    // -------------------------------
    // Public API
    // -------------------------------

    public async Task<List<RecycleBinItem>> GetAllAsync(CancellationToken ct = default)
    {
        var items = new List<RecycleBinItem>(256);

        // 1) Soft-deleted Rows (für alle Entities mit IsDeleted==true & Guid Id)
        items.AddRange(await CollectSoftDeletedAsync(ct));

        // 2) Hard-Deletes (neueste Delete-Revision je (Table, AssetId))
        var deletes = await _db.Set<Revision>()
            .Where(r => r.Action == "Delete")
            .GroupBy(r => new { r.Table, r.AssetId })
            .Select(g => g.OrderByDescending(r => r.CreatedAt).First())
            .ToListAsync(ct);

        foreach (var r in deletes)
        {
            items.Add(new RecycleBinItem
            {
                Table      = r.Table ?? "",
                EntityType = r.Table ?? "",
                AssetId    = r.AssetId,
                Display    = ExtractDisplayFromJson(r.Data),
                DeletedAt  = r.CreatedAt,
                DeletedBy  = r.UserEmail,
                Source     = "Revision"
            });
        }

        // 3) Deduplizieren & Sortieren
        return items
            .GroupBy(i => (NormalizeToMappedTableName(i.Table), i.AssetId))
            .Select(g => g.OrderByDescending(x => x.DeletedAt ?? DateTimeOffset.MinValue).First())
            .OrderByDescending(i => i.DeletedAt ?? DateTimeOffset.MinValue)
            .ToList();
    }

    public async Task RestoreAsync(string table, Guid id, string? userEmail = null, CancellationToken ct = default)
    {
        // 1) Über Delete-Revision wiederherstellen (falls vorhanden)
        var revId = GetLatestDeleteRevisionIdOrDefault(table, id);
        if (revId.HasValue)
        {
            await _revisions.RestoreAsync(revisionId: revId.Value, userEmail: userEmail, ct: ct);
            return;
        }

        // 2) Fallback: Soft-Delete rückgängig machen (generisch)
        var et = FindEntityType(table);
        if (et is null)
            throw new NotSupportedException($"Restore: Tabelle/Entity '{table}' ist dem EF-Modell nicht bekannt.");

        var entity = await FindEntityByIdAsync(et, id, ct, ignoreQueryFilters: true);
        if (entity is null)
            throw new InvalidOperationException("Element nicht gefunden.");

        SetIfExists(entity, "IsDeleted", false);
        SetIfExists(entity, "DeletedAt", null);
        SetIfExists(entity, "DeletedBy", null);

        _db.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task PurgeAsync(string table, Guid id, CancellationToken ct = default)
    {
        var et = FindEntityType(table);
        if (et is null)
            throw new NotSupportedException($"Purge: Tabelle/Entity '{table}' ist dem EF-Modell nicht bekannt.");

        var mappedTable = TableNameOf(et);

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            // 1) Datensatz physisch löschen (falls er noch existiert)
            var entity = await FindEntityByIdAsync(et, id, ct, ignoreQueryFilters: true);
            if (entity != null)
            {
                _db.Remove(entity);
                await _db.SaveChangesAsync(ct);
            }

            // 2) Sämtliche Revisions zu (Table + AssetId) löschen
            await _db.Set<Revision>()
                .Where(r => r.Table == mappedTable && r.AssetId == id)
                .ExecuteDeleteAsync(ct);

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // -------------------------------
    // Soft-Delete Sammlung (generisch)
    // -------------------------------

    private async Task<List<RecycleBinItem>> CollectSoftDeletedAsync(CancellationToken ct)
    {
        var result = new List<RecycleBinItem>();

        foreach (var et in _db.Model.GetEntityTypes())
        {
            // Konvention: Guid Id + bool IsDeleted
            var idProp = et.FindProperty("Id");
            var isDeletedProp = et.FindProperty("IsDeleted");
            if (idProp?.ClrType != typeof(Guid) || isDeletedProp?.ClrType != typeof(bool))
                continue;

            var clr = et.ClrType;

            // IQueryable für den Set<T> erzeugen und QueryFilters ignorieren
            var setQuery = GetSetQueryable(clr);
            var ignored = IgnoreQueryFilters(setQuery, clr);

            // where e => EF.Property<bool>(e, "IsDeleted") == true
            var param = Expression.Parameter(clr, "e");
            var isDelAccess = Expression.Call(
                typeof(EF), nameof(EF.Property), new[] { typeof(bool) }, param, Expression.Constant("IsDeleted"));
            var predicate = Expression.Lambda(
                Expression.Equal(isDelAccess, Expression.Constant(true)), param);

            var whereExpr = Expression.Call(
                typeof(Queryable), nameof(Queryable.Where), new[] { clr }, ignored.Expression, predicate);

            var whereQuery = ignored.Provider.CreateQuery(whereExpr);

            var list = await ToListAsync(whereQuery, clr, ct); // IList

            foreach (var entity in list)
            {
                var id = (Guid)Get(entity, "Id")!;
                var deletedAt = Get(entity, "DeletedAt") as DateTimeOffset?
                                ?? (Get(entity, "DeletedAt") as DateTime?)?.ToUniversalTime();
                var deletedBy = Get(entity, "DeletedBy") as string;

                var display = ExtractDisplayFromEntity(entity);

                result.Add(new RecycleBinItem
                {
                    Table      = TableNameOf(et),
                    EntityType = clr.Name,
                    AssetId    = id,
                    Display    = display ?? "(ohne Bezeichnung)",
                    DeletedAt  = deletedAt,
                    DeletedBy  = deletedBy,
                    Source     = "Row"
                });
            }
        }

        return result;
    }

    // -------------------------------
    // Helpers
    // -------------------------------

    private long? GetLatestDeleteRevisionIdOrDefault(string tableOrClr, Guid id)
    {
        var mapped = NormalizeToMappedTableName(tableOrClr);
        var rev = _db.Set<Revision>()
            .Where(r => r.Table == mapped && r.AssetId == id && r.Action == "Delete")
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefault();
        return rev?.Id;
    }

    private string NormalizeToMappedTableName(string tableOrClr)
    {
        var et = FindEntityType(tableOrClr);
        return et != null ? TableNameOf(et) : (tableOrClr?.Trim() ?? "");
    }

    private static string ExtractDisplayFromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return "(ohne Bezeichnung)";
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (TryStr(root, "Name", out var s)) return s;
            if (TryStr(root, "Title", out s)) return s;
            if (TryStr(root, "Email", out s)) return s;
            if (TryStr(root, "Tag", out s)) return s;

            return "(Snapshot)";
        }
        catch
        {
            return "(Snapshot)";
        }

        static bool TryStr(JsonElement e, string key, out string val)
        {
            val = "";
            if (e.ValueKind != JsonValueKind.Object) return false;
            if (!e.TryGetProperty(key, out var p)) return false;
            if (p.ValueKind != JsonValueKind.String) return false;
            var s = p.GetString();
            if (string.IsNullOrWhiteSpace(s)) return false;
            val = s!;
            return true;
        }
    }

    private static string ExtractDisplayFromEntity(object entity)
    {
        var t = entity.GetType();

        string? pick(params string[] names)
        {
            foreach (var n in names)
            {
                var p = t.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (p?.PropertyType == typeof(string))
                {
                    var val = p.GetValue(entity) as string;
                    if (!string.IsNullOrWhiteSpace(val)) return val!;
                }
            }
            return null;
        }

        var baseText = pick("Name", "Title", "Email", "Tag") ?? "(ohne Bezeichnung)";

        var verProp = t.GetProperty("Version", BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (verProp?.PropertyType == typeof(string))
        {
            var v = verProp.GetValue(entity) as string;
            if (!string.IsNullOrWhiteSpace(v)) return $"{baseText} {v}";
        }

        return baseText;
    }

    private IEntityType? FindEntityType(string tableOrClrName)
    {
        var normalized = tableOrClrName?.Trim() ?? "";

        // 1) Exakter Tabellenname (EF-Mapping)
        var et = _db.Model.GetEntityTypes()
            .FirstOrDefault(e => string.Equals(TableNameOf(e), normalized, StringComparison.OrdinalIgnoreCase));
        if (et != null) return et;

        // 2) CLR-Klassenname (z. B. "LibraryFramework")
        et = _db.Model.GetEntityTypes()
            .FirstOrDefault(e => string.Equals(e.ClrType.Name, normalized, StringComparison.OrdinalIgnoreCase));
        if (et != null) return et;

        return null;
    }

    private static string TableNameOf(IEntityType et)
        => et.GetTableName() ?? et.ClrType.Name;

    private async Task<object?> FindEntityByIdAsync(IEntityType et, Guid id, CancellationToken ct, bool ignoreQueryFilters)
    {
        var clr = et.ClrType;

        // DbSet<T> als IQueryable besorgen
        var setQuery = GetSetQueryable(clr);
        if (ignoreQueryFilters)
            setQuery = IgnoreQueryFilters(setQuery, clr);

        // where e => EF.Property<Guid>(e,"Id") == id
        var param = Expression.Parameter(clr, "e");
        var idAccess = Expression.Call(
            typeof(EF), nameof(EF.Property), new[] { typeof(Guid) }, param, Expression.Constant("Id"));
        var predicate = Expression.Lambda(
            Expression.Equal(idAccess, Expression.Constant(id)), param);

        var whereExpr = Expression.Call(
            typeof(Queryable), nameof(Queryable.Where), new[] { clr }, setQuery.Expression, predicate);
        var whereQuery = setQuery.Provider.CreateQuery(whereExpr);

        // FirstOrDefaultAsync<T>(IQueryable<T>, CancellationToken)
        var firstOrDefaultAsync = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(EntityFrameworkQueryableExtensions.FirstOrDefaultAsync)
                        && m.GetParameters().Length == 2)
            .MakeGenericMethod(clr);

        var task = (Task)firstOrDefaultAsync.Invoke(null, new object[] { whereQuery, ct })!;
        await task.ConfigureAwait(false);
        return task.GetType().GetProperty("Result")!.GetValue(task);
    }

    // --- Reflection-basierte Non-Generic Helpers ---

    private IQueryable GetSetQueryable(Type entityClr)
    {
        // DbContext.Set<TEntity>()
        var method = typeof(DbContext).GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .First(m => m.Name == nameof(DbContext.Set) && m.IsGenericMethodDefinition && m.GetParameters().Length == 0);
        var generic = method.MakeGenericMethod(entityClr);
        var dbSetObj = generic.Invoke(_db, null)!; // DbSet<TEntity>
        return (IQueryable)dbSetObj;
    }

    private static IQueryable IgnoreQueryFilters(IQueryable source, Type elementType)
    {
        // EntityFrameworkQueryableExtensions.IgnoreQueryFilters<T>(IQueryable<T>)
        var method = typeof(EntityFrameworkQueryableExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(EntityFrameworkQueryableExtensions.IgnoreQueryFilters)
                        && m.IsGenericMethodDefinition
                        && m.GetParameters().Length == 1);
        var generic = method.MakeGenericMethod(elementType);
        return (IQueryable)generic.Invoke(null, new object[] { source })!;
    }

    private static object? Get(object entity, string propName)
        => entity.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)
                 ?.GetValue(entity);

    private static void SetIfExists(object entity, string propName, object? value)
    {
        var p = entity.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (p is null || !p.CanWrite) return;
        p.SetValue(entity, value);
    }

    private static async Task<IList> ToListAsync(IQueryable query, Type elementType, CancellationToken ct)
    {
        // EntityFrameworkQueryableExtensions.ToListAsync<T>(IQueryable<T>, CancellationToken)
        var toListAsync = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(EntityFrameworkQueryableExtensions.ToListAsync)
                        && m.IsGenericMethodDefinition
                        && m.GetParameters().Length == 2)
            .MakeGenericMethod(elementType);

        var task = (Task)toListAsync.Invoke(null, new object[] { query, ct })!;
        await task.ConfigureAwait(false);
        return (IList)task.GetType().GetProperty("Result")!.GetValue(task)!;
    }
}

// DTO/Contract
public sealed class RecycleBinItem
{
    public string Table { get; set; } = "";
    public string EntityType { get; set; } = "";
    public Guid AssetId { get; set; }
    public string Display { get; set; } = "";
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public string Source { get; set; } = ""; // "Row" | "Revision"
}
