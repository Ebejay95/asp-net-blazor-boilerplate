using System;
using System.Linq;
using System.Collections.Generic;
using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CMC.Infrastructure.Persistence.Configurations;

public class LibraryControlConfiguration : IEntityTypeConfiguration<LibraryControl>
{
    // --- statische Helper für ValueComparer (keine verbotenen Expression-Features) ---
    private static bool DepsEquals(IReadOnlyList<string> l, IReadOnlyList<string> r)
    {
        if (ReferenceEquals(l, r)) return true;
        if (l is null || r is null) return false;
        if (l.Count != r.Count) return false;
        for (int i = 0; i < l.Count; i++)
            if (!string.Equals(l[i], r[i], StringComparison.Ordinal)) return false;
        return true;
    }

    private static int DepsHash(IReadOnlyList<string> v)
    {
        if (v is null) return 0;
        unchecked
        {
            int hash = 17;
            for (int i = 0; i < v.Count; i++)
                hash = hash * 23 + (v[i]?.GetHashCode() ?? 0);
            return hash;
        }
    }

    private static IReadOnlyList<string> DepsSnapshot(IReadOnlyList<string> v)
        => v is null ? null : v.ToArray();

    public void Configure(EntityTypeBuilder<LibraryControl> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id).HasMaxLength(16); // "C001" etc.
        entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        entity.Property(e => e.Tag).IsRequired().HasMaxLength(100);
        entity.Property(e => e.CapexEur).HasPrecision(18, 2);
        entity.Property(e => e.OpexYearEur).HasPrecision(18, 2);
        entity.Property(e => e.IntDays).IsRequired();
        entity.Property(e => e.ExtDays).IsRequired();
        entity.Property(e => e.TtlDays).IsRequired();
        entity.Property(e => e.Industry).HasMaxLength(100);

        // Deps: Postgres text[] + Converter + ValueComparer
        var depsConverter = new ValueConverter<IReadOnlyList<string>, string[]>(
            toProvider   => (toProvider ?? Array.Empty<string>()).ToArray(),
            fromProvider => (fromProvider ?? Array.Empty<string>())
        );

        var depsComparer = new ValueComparer<IReadOnlyList<string>>(
            (l, r) => DepsEquals(l, r),
            v => DepsHash(v),
            v => DepsSnapshot(v)
        );

        // Kein Method-Chaining nach SetValueComparer (ist void)
        var depsProp = entity.Property(e => e.Deps).HasConversion(depsConverter);
        depsProp.Metadata.SetValueComparer(depsComparer);
        depsProp.HasColumnType("text[]");

        entity.Property(e => e.CreatedAt).IsRequired();
        entity.Property(e => e.UpdatedAt).IsRequired();

        entity.HasIndex(e => e.Tag);
        entity.HasIndex(e => e.Industry);
    }
}
