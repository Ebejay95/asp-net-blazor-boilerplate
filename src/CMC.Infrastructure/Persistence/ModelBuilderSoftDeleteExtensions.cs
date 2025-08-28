using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using CMC.Domain.Entities;

namespace CMC.Infrastructure.Persistence;

public static class ModelBuilderSoftDeleteExtensions
{
    /// <summary>
    /// Fügt für alle EntityTypes, die ISoftDeletable implementieren,
    /// einen globalen QueryFilter (IsDeleted == false) hinzu.
    /// </summary>
    public static void ApplySoftDeleteFilters(this ModelBuilder modelBuilder)
    {
        foreach (var et in modelBuilder.Model.GetEntityTypes())
        {
            var clr = et.ClrType;
            if (!typeof(ISoftDeletable).IsAssignableFrom(clr)) continue;

            var param = Expression.Parameter(clr, "e");
            var prop = Expression.Property(param, nameof(ISoftDeletable.IsDeleted));
            var body = Expression.Equal(prop, Expression.Constant(false));
            var lambda = Expression.Lambda(body, param);

            et.SetQueryFilter(lambda);
            // optional: Soft-Delete-Spalten als Shadow-Props erzwingen
        }
    }
}
