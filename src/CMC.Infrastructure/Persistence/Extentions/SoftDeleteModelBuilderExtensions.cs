using System;
using System.Linq.Expressions;
using System.Reflection;
using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Persistence.Extensions
{
    public static class SoftDeleteModelBuilderExtensions
    {
        /// <summary>
        /// Setzt für alle ISoftDeletable-Entities einen globalen Filter.
        /// Unterstützt entweder bool IsDeleted oder Nullable DeletedAt (DateTime/DateTimeOffset).
        /// </summary>
        public static void ApplySoftDeleteFilters(this ModelBuilder modelBuilder)
        {
            foreach (var et in modelBuilder.Model.GetEntityTypes())
            {
                var clr = et.ClrType;
                if (!typeof(ISoftDeletable).IsAssignableFrom(clr))
                    continue;

                // Property-Erkennung
                var isDeleted = clr.GetProperty("IsDeleted", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                var deletedAt = clr.GetProperty("DeletedAt", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                // Baue (e => !e.IsDeleted) ODER (e => e.DeletedAt == null)
                var p = Expression.Parameter(clr, "e");
                Expression body;

                if (isDeleted != null && isDeleted.PropertyType == typeof(bool))
                {
                    body = Expression.Equal(Expression.Property(p, isDeleted), Expression.Constant(false));
                }
                else if (deletedAt != null && Nullable.GetUnderlyingType(deletedAt.PropertyType) != null)
                {
                    body = Expression.Equal(Expression.Property(p, deletedAt), Expression.Constant(null, deletedAt.PropertyType));
                }
                else
                {
                    // Falls das Interface implementiert wird, aber keins der Felder existiert -> weiter
                    continue;
                }

                var lambda = Expression.Lambda(body, p);

                // EF Core 8: non-generic Overload akzeptiert LambdaExpression
                modelBuilder.Entity(clr).HasQueryFilter(lambda);
            }
        }
    }
}
