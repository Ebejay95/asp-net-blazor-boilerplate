// src/CMC.Infrastructure/Services/RevisionService.cs
using System.Globalization;
using System.Text.Json;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Services;

public sealed class RevisionService
{
    private static readonly HashSet<string> IgnoreOnRestore = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id", "CreatedAt", "UpdatedAt", "DeletedAt", "DeletedBy", "IsDeleted", "Version"
    };

    private readonly AppDbContext _db;

    public RevisionService(AppDbContext db) => _db = db;

    public Task<List<Revision>> GetByAssetAsync(string table, Guid id, int take = 100)
        => _db.Set<Revision>()
              .Where(r => r.Table == table && r.AssetId == id)
              .OrderByDescending(r => r.CreatedAt)
              .Take(take)
              .ToListAsync();

    // Alias method for compatibility with EfRevisionsClient
    public Task<List<Revision>> GetAsync(string table, Guid id, int take = 100)
        => GetByAssetAsync(table, id, take);

    public async Task RestoreAsync(long revisionId, string? userEmail = null, CancellationToken ct = default)
    {
        // Revision nur lesen, kein Tracking nötig
        var rev = await _db.Set<Revision>()
                           .AsNoTracking()
                           .FirstOrDefaultAsync(r => r.Id == revisionId, ct)
                  ?? throw new InvalidOperationException("Revision nicht gefunden.");

        // EntityType über Tabellenname finden
        var et = _db.Model.GetEntityTypes()
            .FirstOrDefault(t =>
            {
                var schema = t.GetSchema();
                var table  = t.GetTableName();
                var full   = (string.IsNullOrEmpty(schema) ? table : $"{schema}.{table}") ?? "";
                return string.Equals(full, rev.Table, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(table, rev.Table, StringComparison.OrdinalIgnoreCase);
            })
            ?? throw new InvalidOperationException($"EntityType zu '{rev.Table}' nicht gefunden.");

        var clr = et.ClrType;

        // Snapshot -> Dictionary (robust gegen null/leer/invalid)
        var dict = SafeToDictionary(rev.Data);

        // Bestehende Entity laden/erzeugen
        var existing = await _db.FindAsync(clr, new object?[] { rev.AssetId }, ct);

        if (existing is null)
        {
            existing = Activator.CreateInstance(clr, nonPublic: true)
                       ?? throw new InvalidOperationException($"Instanz von {clr.Name} konnte nicht erzeugt werden.");

            var idProp = clr.GetProperty("Id");
            if (idProp is not null && idProp.PropertyType == typeof(Guid))
                idProp.SetValue(existing, rev.AssetId);

            _db.Add(existing);
        }

        var entry = _db.Entry(existing);

        // Skalare Properties setzen
        foreach (var propEntry in entry.Properties)
        {
            var name = propEntry.Metadata.Name;

            if (IgnoreOnRestore.Contains(name))
                continue;

            if (!dict.TryGetValue(name, out var raw))
                continue;

            try
            {
                var targetType = propEntry.Metadata.ClrType;
                var converted  = ConvertJsonValue(raw, targetType);
                propEntry.CurrentValue = converted;
            }
            catch (Exception ex)
            {
                // optionales Logging
                Console.WriteLine($"[Restore] Warn: Property '{name}' konnte nicht gesetzt werden: {ex.Message}");
            }
        }

        // Soft-Delete rückgängig machen
        if (existing is ISoftDeletable sd)
        {
            sd.IsDeleted = false;
            sd.DeletedAt = null;
            sd.DeletedBy = null;
        }
        // if (writeRevision)
        // {
        //     _db.Set<Revision>().Add(new Revision
        //     {
        //         Table = rev.Table ?? string.Empty,
        //         AssetId = rev.AssetId,
        //         Action = "Restore",
        //         UserEmail = userEmail,
        //         Data = rev.Data ?? "{}",
        //         CreatedAt = DateTimeOffset.UtcNow
        //     });
        // }
        // *** WICHTIG: KEINE explizite Revision "Restore" mehr schreiben! ***
        // Dein Audit/ChangeTracker erzeugt beim SaveChanges ein normales "Update".
        await _db.SaveChangesAsync(ct);
    }

    // Fixed method - assuming you want to get a revision by ID and return it as RecycleBinItem
    public async Task<RecycleBinItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var revision = await _db.Set<Revision>()
                                .FirstOrDefaultAsync(r => r.AssetId == id, ct);

        return revision is null ? null : MapToRecycleBinItem(revision);
    }

    // Helper method to map Revision to RecycleBinItem
    private static RecycleBinItem MapToRecycleBinItem(Revision revision)
    {
        return new RecycleBinItem
        {
            Table = revision.Table ?? string.Empty,
            EntityType = revision.Table ?? string.Empty,
            AssetId = revision.AssetId,
            Display = ExtractDisplayFromJson(revision.Data),
            DeletedAt = revision.CreatedAt,
            DeletedBy = revision.UserEmail,
            Source = "Revision"
        };
    }

    // Helper method to extract display text from JSON (copied from RecycleBinService)
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

    private static Dictionary<string, object?> SafeToDictionary(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(
                       json, new JsonSerializerOptions(JsonSerializerDefaults.Web)
                   ) ?? new();
        }
        catch
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                    foreach (var p in doc.RootElement.EnumerateObject())
                        dict[p.Name] = p.Value.Clone(); // als JsonElement behalten
                    return dict;
                }
            }
            catch { /* ignorieren */ }
            return new();
        }
    }

    private static object? ConvertJsonValue(object? value, Type targetType)
    {
        if (value is null) return null;

        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (value is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.String)
            {
                var s = je.GetString();

                if (underlying == typeof(Guid))
                    return Guid.TryParse(s, out var g) ? g : null;

                if (underlying == typeof(DateTimeOffset))
                {
                    if (DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dto))
                        return dto;
                    if (DateTime.TryParse(s, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                        return new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc));
                    return null;
                }

                if (underlying == typeof(DateTime))
                {
                    if (DateTime.TryParse(s, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                        return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                    if (DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dto))
                        return DateTime.SpecifyKind(dto.UtcDateTime, DateTimeKind.Utc);
                    return null;
                }

                if (underlying.IsEnum && s is not null)
                    return Enum.TryParse(underlying, s, ignoreCase: true, out var ev) ? ev : null;

                if (underlying == typeof(string)) return s;
            }

            if (je.ValueKind == JsonValueKind.Number)
            {
                if (underlying == typeof(int)    && je.TryGetInt32(out var i)) return i;
                if (underlying == typeof(long)   && je.TryGetInt64(out var l)) return l;
                if (underlying == typeof(decimal)&& je.TryGetDecimal(out var m)) return m;
                if (underlying == typeof(double) && je.TryGetDouble(out var d)) return d;
                if (underlying == typeof(float)  && je.TryGetDouble(out var f)) return (float)f;
            }

            if (je.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                if (underlying == typeof(bool)) return je.GetBoolean();
            }

            // Fallback: JSON → Zieltyp
            return je.Deserialize(underlying, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }

        if (underlying.IsInstanceOfType(value)) return value;

        if (underlying == typeof(Guid) && value is string gs)
            return Guid.TryParse(gs, out var g2) ? g2 : null;

        if (underlying == typeof(DateTimeOffset))
        {
            if (value is DateTimeOffset dto) return dto;
            if (value is DateTime dt) return new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc));
            if (value is string s &&
                DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dto2))
                return dto2;
            return null;
        }

        if (underlying == typeof(DateTime))
        {
            if (value is DateTime dt) return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            if (value is DateTimeOffset dto) return DateTime.SpecifyKind(dto.UtcDateTime, DateTimeKind.Utc);
            if (value is string s &&
                DateTime.TryParse(s, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt2))
                return DateTime.SpecifyKind(dt2, DateTimeKind.Utc);
            return null;
        }

        try
        {
            return Convert.ChangeType(value, underlying, CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }
}
