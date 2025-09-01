using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CMC.Web.Services
{
    public enum RelationKind { Reference, OneToMany, ManyToMany }

    public sealed record RelationOption(string Label, string Value);

    public sealed class RelationDescriptor
    {
        public required string Name { get; init; }
        public required RelationKind Kind { get; init; }
        public required Type ParentType { get; init; }
        public required Type TargetType { get; init; }

        public required Func<Task<IEnumerable<RelationOption>>> LoadOptions { get; init; }
        public required Func<object, Task<IEnumerable<string>>> LoadCurrentKeys { get; init; }

        public Func<object, string, Task>? SetReference { get; init; }
        public Func<object, IEnumerable<string>, Task>? AddMany { get; init; }
        public Func<object, IEnumerable<string>, Task>? RemoveMany { get; init; }
    }

    public interface IRelationshipManager
    {
        RelationDescriptor GetDescriptor(Type parentType, string relationName);
        RelationDescriptor GetDescriptor<TParent>(string relationName) where TParent : class;
        Task<object> FindParentAsync(Type parentType, object key);
    }

    /// <summary>
    /// Generischer Relationship-Explorer + Mutator für EF Core (1:1, 1:n, n:m inkl. expliziter Join-Entity).
    /// WICHTIG: Alle DB-Operationen laufen seriell über ein Semaphore – verhindert
    /// "A second operation was started on this context…" wenn mehrere Checkboxlisten parallel laden.
    /// </summary>
    public sealed class RelationshipManager<TDbContext> : IRelationshipManager where TDbContext : DbContext
    {
        private readonly TDbContext _db;
        private readonly SemaphoreSlim _gate = new(1, 1);

        public RelationshipManager(TDbContext db) { _db = db; }

        public RelationDescriptor GetDescriptor<TParent>(string relationName) where TParent : class
            => GetDescriptor(typeof(TParent), relationName);

        public RelationDescriptor GetDescriptor(Type parentType, string relationName)
        {
            var model = _db.Model;
            var parentEt = model.FindEntityType(parentType)
                ?? throw new InvalidOperationException($"EntityType not found: {parentType.Name}");

            // 1) Normale Navigation (Reference oder Collection)
            var nav = parentEt.FindNavigation(relationName);
            if (nav is not null)
            {
                var targetEt = nav.TargetEntityType;

                // Explizite Join-Collection: Collection<JoinEntity> mit genau 2 FKs
                if (nav.IsCollection && LooksLikeExplicitJoinCollection(parentEt, nav.TargetEntityType))
                {
                    var fks = nav.TargetEntityType.GetForeignKeys().ToList();
                    var fkToParent = fks.FirstOrDefault(fk => fk.PrincipalEntityType == parentEt);
                    var fkToTarget = fks.FirstOrDefault(fk => fk.PrincipalEntityType != parentEt)
                                     ?? throw new InvalidOperationException($"Join '{relationName}' on {parentType.Name} has no second FK.");
                    var targetEtFromJoin = fkToTarget.PrincipalEntityType;

                    return BuildManyToManyViaExplicitJoin(
                        parentType,
                        relationName,
                        nav.TargetEntityType,
                        fkToParent!,
                        fkToTarget,
                        targetEtFromJoin);
                }

                return nav.IsCollection
                    ? BuildOneToMany(parentType, relationName, targetEt)
                    : BuildReference(parentType, relationName, targetEt);
            }

            // 2) Skip-Navigation (echtes n:m)
            var skip = parentEt.FindSkipNavigation(relationName);
            if (skip is not null)
                return BuildManyToMany(parentType, relationName, skip.TargetEntityType);

            // 3) Fallback: Join-Collection per Namen finden
            var joinNav = parentEt.GetNavigations()
                .FirstOrDefault(n => n.IsCollection && string.Equals(n.Name, relationName, StringComparison.Ordinal));

            if (joinNav is not null && LooksLikeExplicitJoinCollection(parentEt, joinNav.TargetEntityType))
            {
                var fks = joinNav.TargetEntityType.GetForeignKeys().ToList();
                var fkToParent = fks.First(fk => fk.PrincipalEntityType == parentEt);
                var fkToTarget = fks.First(fk => fk.PrincipalEntityType != parentEt);
                var targetEt = fkToTarget.PrincipalEntityType;
                return BuildManyToManyViaExplicitJoin(parentType, relationName, joinNav.TargetEntityType, fkToParent, fkToTarget, targetEt);
            }

            throw new InvalidOperationException($"Relation '{relationName}' not found on {parentType.Name}");
        }

        public async Task<object> FindParentAsync(Type parentType, object key)
        {
            await _gate.WaitAsync();
            try
            {
                var et = _db.Model.FindEntityType(parentType)
                         ?? throw new InvalidOperationException($"EntityType not found: {parentType.Name}");

                var pkProp = et.FindPrimaryKey()!.Properties.Single();
                var pkClr = pkProp.ClrType;

                object? typed = key;
                if (key is string s)
                {
                    typed = pkClr == typeof(Guid) || pkClr == typeof(Guid?)
                        ? Guid.Parse(s)
                        : Convert.ChangeType(s, Nullable.GetUnderlyingType(pkClr) ?? pkClr);
                }

                var entity = await _db.FindAsync(parentType, new object?[] { typed! });
                if (entity is null) throw new InvalidOperationException($"{parentType.Name}({key}) not found");
                return entity;
            }
            finally { _gate.Release(); }
        }

        // ------------------------ Builder ------------------------

        private RelationDescriptor BuildReference(Type parentType, string rel, IEntityType targetEt)
        {
            var targetClr = targetEt.ClrType;

            return new RelationDescriptor
            {
                Name = rel,
                Kind = RelationKind.Reference,
                ParentType = parentType,
                TargetType = targetClr,

                LoadOptions = async () =>
                {
                    await _gate.WaitAsync();
                    try
                    {
                        var set = GetSetNonGeneric(targetClr);
                        var list = await ToListAsync(set);
                        return list.Select(e => new RelationOption(GetDisplay(e), GetKey(e)!));
                    }
                    finally { _gate.Release(); }
                },

                LoadCurrentKeys = async parent =>
                {
                    await _gate.WaitAsync();
                    try
                    {
                        await _db.Entry(parent).Reference(rel).LoadAsync();
                        var obj = _db.Entry(parent).Reference(rel).CurrentValue;
                        return obj is null ? Array.Empty<string>() : new[] { GetKey(obj)! };
                    }
                    finally { _gate.Release(); }
                },

                SetReference = async (parent, key) =>
                {
                    await _gate.WaitAsync();
                    try
                    {
                        var entity = string.IsNullOrWhiteSpace(key) ? null : await FindByKey(targetClr, key);
                        _db.Entry(parent).Reference(rel).CurrentValue = entity;
                        await _db.SaveChangesAsync();
                    }
                    finally { _gate.Release(); }
                }
            };
        }

        private RelationDescriptor BuildOneToMany(Type parentType, string rel, IEntityType targetEt)
        {
            return new RelationDescriptor
            {
                Name = rel,
                Kind = RelationKind.OneToMany,
                ParentType = parentType,
                TargetType = targetEt.ClrType,

                LoadOptions = () => Task.FromResult<IEnumerable<RelationOption>>(Array.Empty<RelationOption>()),

                LoadCurrentKeys = async parent =>
                {
                    await _gate.WaitAsync();
                    try
                    {
                        await _db.Entry(parent).Collection(rel).LoadAsync();
                        var children = (IEnumerable<object>)(_db.Entry(parent).Collection(rel).CurrentValue ?? Array.Empty<object>());
                        return children.Select(GetKey!).Where(k => k is not null)!;
                    }
                    finally { _gate.Release(); }
                }
            };
        }

        private RelationDescriptor BuildManyToMany(Type parentType, string rel, IEntityType targetEt)
        {
            var targetClr = targetEt.ClrType;

            return new RelationDescriptor
            {
                Name = rel,
                Kind = RelationKind.ManyToMany,
                ParentType = parentType,
                TargetType = targetClr,

                LoadOptions = async () =>
                {
                    await _gate.WaitAsync();
                    try
                    {
                        var set = GetSetNonGeneric(targetClr);
                        var list = await ToListAsync(set);
                        return list.Select(e => new RelationOption(GetDisplay(e), GetKey(e)!));
                    }
                    finally { _gate.Release(); }
                },

                LoadCurrentKeys = async parent =>
                {
                    await _gate.WaitAsync();
                    try
                    {
                        await _db.Entry(parent).Collection(rel).LoadAsync();
                        var curr = (IEnumerable<object>)(_db.Entry(parent).Collection(rel).CurrentValue ?? Array.Empty<object>());
                        return curr.Select(GetKey!).Where(k => k is not null)!;
                    }
                    finally { _gate.Release(); }
                },

                AddMany = async (parent, keys) =>
                {
                    if (await TryUseDomainMethodAsync(parent, rel, "Add", keys))
                        return;

                    await _gate.WaitAsync();
                    try
                    {
                        await _db.Entry(parent).Collection(rel).LoadAsync();
                        var coll = (IList)_db.Entry(parent).Collection(rel).CurrentValue!;

                        foreach (var k in keys)
                        {
                            var entity = await FindByKey(targetClr, k);
                            if (!coll.Contains(entity))
                                coll.Add(entity);
                        }

                        await _db.SaveChangesAsync();
                    }
                    finally { _gate.Release(); }
                },

                RemoveMany = async (parent, keys) =>
                {
                    if (await TryUseDomainMethodAsync(parent, rel, "Remove", keys))
                        return;

                    await _gate.WaitAsync();
                    try
                    {
                        await _db.Entry(parent).Collection(rel).LoadAsync();
                        var coll = (IList)_db.Entry(parent).Collection(rel).CurrentValue!;

                        foreach (var k in keys.ToList())
                        {
                            var entity = await FindByKey(targetClr, k);
                            if (coll.Contains(entity))
                                coll.Remove(entity);
                        }

                        await _db.SaveChangesAsync();
                    }
                    finally { _gate.Release(); }
                }
            };
        }

        private RelationDescriptor BuildManyToManyViaExplicitJoin(
            Type parentType,
            string joinCollectionName,
            IEntityType joinEt,
            IForeignKey fkToParent,
            IForeignKey fkToTarget,
            IEntityType targetEt)
        {
            var joinClr = joinEt.ClrType;
            var targetClr = targetEt.ClrType;

            var fkParentProp = fkToParent.Properties.Single().PropertyInfo!;
            var fkTargetProp = fkToTarget.Properties.Single().PropertyInfo!;
            var parentPkProp = _db.Model.FindEntityType(parentType)!.FindPrimaryKey()!.Properties.Single().PropertyInfo!;

            return new RelationDescriptor
            {
                Name = joinCollectionName,
                Kind = RelationKind.ManyToMany,
                ParentType = parentType,
                TargetType = targetClr,

                LoadOptions = async () =>
                {
                    await _gate.WaitAsync();
                    try
                    {
                        var set = GetSetNonGeneric(targetClr);
                        var list = await ToListAsync(set);
                        return list.Select(e => new RelationOption(GetDisplay(e), GetKey(e)!));
                    }
                    finally { _gate.Release(); }
                },

                LoadCurrentKeys = async parent =>
                {
                    // optionaler Domain-Reader
                    var domainKeys = TryGetDomainKeys(parent, joinCollectionName);
                    if (domainKeys != null)
                        return domainKeys.Select(k => k.ToString()).ToArray();

                    await _gate.WaitAsync();
                    try
                    {
                        await _db.Entry(parent).Collection(joinCollectionName).LoadAsync();
                        var coll = (IEnumerable<object>)(_db.Entry(parent).Collection(joinCollectionName).CurrentValue ?? Array.Empty<object>());

                        return coll
                            .Select(j => fkTargetProp.GetValue(j)?.ToString())
                            .Where(s => !string.IsNullOrWhiteSpace(s))!
                            .ToArray()!;
                    }
                    finally { _gate.Release(); }
                },

                AddMany = async (parent, keys) =>
                {
                    if (await TryUseDomainMethodAsync(parent, joinCollectionName, "Add", keys))
                        return;

                    await _gate.WaitAsync();
                    try
                    {
                        await _db.Entry(parent).Collection(joinCollectionName).LoadAsync();
                        var coll = (IList)_db.Entry(parent).Collection(joinCollectionName).CurrentValue!;

                        var parentKeyObj = parentPkProp.GetValue(parent)!;
                        var parentKeyTyped = Convert.ChangeType(parentKeyObj, fkParentProp.PropertyType);

                        var existingTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var j in coll)
                        {
                            var tVal = fkTargetProp.GetValue(j)?.ToString();
                            if (!string.IsNullOrWhiteSpace(tVal))
                                existingTargets.Add(tVal);
                        }

                        foreach (var k in keys)
                        {
                            if (existingTargets.Contains(k)) continue;

                            var targetKeyTyped = Convert.ChangeType(k, fkTargetProp.PropertyType);

                            var joinInstance = Activator.CreateInstance(joinClr)!;
                            fkParentProp.SetValue(joinInstance, parentKeyTyped);
                            fkTargetProp.SetValue(joinInstance, targetKeyTyped);

                            coll.Add(joinInstance);
                        }

                        await _db.SaveChangesAsync();
                    }
                    finally { _gate.Release(); }
                },

                RemoveMany = async (parent, keys) =>
                {
                    if (await TryUseDomainMethodAsync(parent, joinCollectionName, "Remove", keys))
                        return;

                    await _gate.WaitAsync();
                    try
                    {
                        await _db.Entry(parent).Collection(joinCollectionName).LoadAsync();
                        var coll = (IList)_db.Entry(parent).Collection(joinCollectionName).CurrentValue!;

                        var keySet = new HashSet<string>(keys, StringComparer.OrdinalIgnoreCase);

                        for (int i = coll.Count - 1; i >= 0; i--)
                        {
                            var ji = coll[i]!;
                            var tId = fkTargetProp.GetValue(ji)?.ToString();
                            if (!string.IsNullOrWhiteSpace(tId) && keySet.Contains(tId))
                                coll.RemoveAt(i);
                        }

                        await _db.SaveChangesAsync();
                    }
                    finally { _gate.Release(); }
                }
            };
        }

        // ------------------------ Domain Convention Helpers ------------------------

        private async Task<bool> TryUseDomainMethodAsync(object parent, string relationName, string operation, IEnumerable<string> keys)
        {
            var relationSuffix = GetRelationSuffix(relationName);
            var methodName = $"{operation}{relationSuffix}";

            var method = parent.GetType().GetMethod(methodName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(IEnumerable<Guid>) },
                null);

            if (method != null)
            {
                try
                {
                    var guids = keys.Select(k => Guid.TryParse(k, out var g) ? g : Guid.Empty)
                                    .Where(g => g != Guid.Empty)
                                    .ToArray();

                    await _gate.WaitAsync();
                    try
                    {
                        method.Invoke(parent, new object[] { guids });
                        await _db.SaveChangesAsync();
                    }
                    finally { _gate.Release(); }

                    return true;
                }
                catch
                {
                    // Domain-Methode fehlgeschlagen → Fallback über EF
                }
            }

            return false;
        }

        private IEnumerable<Guid>? TryGetDomainKeys(object parent, string relationName)
        {
            var relationSuffix = GetRelationSuffix(relationName);
            var methodName = $"Get{relationSuffix}Keys";

            var method = parent.GetType().GetMethod(methodName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                Type.EmptyTypes,
                null);

            if (method != null && typeof(IEnumerable<Guid>).IsAssignableFrom(method.ReturnType))
            {
                try
                {
                    var result = method.Invoke(parent, null);
                    if (result is IEnumerable<Guid> keys)
                        return keys;
                }
                catch { /* ignore */ }
            }

            return null;
        }

        private static string GetRelationSuffix(string relationName)
        {
            var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "CustomerIndustries", "Industries" },
                { "UserRoles", "Roles" },
                { "OrderItems", "Items" },
                { "ProductCategories", "Categories" }
            };

            if (mappings.TryGetValue(relationName, out var mapped))
                return mapped;

            var commonParents = new[] { "Customer", "User", "Order", "Product", "Company", "LibraryControl", "LibraryScenario" };
            foreach (var parent in commonParents)
            {
                if (relationName.StartsWith(parent, StringComparison.OrdinalIgnoreCase) &&
                    relationName.Length > parent.Length)
                    return relationName[parent.Length..];
            }

            return relationName;
        }

        // ------------------------ EF Helpers ------------------------

        private static bool LooksLikeExplicitJoinCollection(IEntityType parentEt, IEntityType joinEt)
        {
            var fks = joinEt.GetForeignKeys().ToList();
            if (fks.Count != 2) return false;
            return fks.Any(fk => fk.PrincipalEntityType == parentEt);
        }

        private static async Task<List<object>> ToListAsync(object set)
        {
            var entityClr = set.GetType().GetGenericArguments()[0];

            var toListAsync = typeof(EntityFrameworkQueryableExtensions)
                .GetMethods()
                .First(m => m.Name == "ToListAsync"
                            && m.IsGenericMethodDefinition
                            && m.GetParameters().Length == 2)
                .MakeGenericMethod(entityClr);

            var query = (IQueryable)set;
            var task = (Task)toListAsync.Invoke(null, new object[] { query, CancellationToken.None })!;
            await task.ConfigureAwait(false);

            var resultProp = task.GetType().GetProperty("Result")!;
            return ((IEnumerable<object>)resultProp.GetValue(task)!).ToList();
        }

        private async Task<object> FindByKey(Type clr, string key)
        {
            await _gate.WaitAsync();
            try
            {
                var et    = _db.Model.FindEntityType(clr)!;
                var pk    = et.FindPrimaryKey()!.Properties.Single();
                var pkClr = pk.ClrType;

                object typed = (pkClr == typeof(Guid) || pkClr == typeof(Guid?))
                    ? Guid.Parse(key)
                    : Convert.ChangeType(key, Nullable.GetUnderlyingType(pkClr) ?? pkClr)!;

                var entity = await _db.FindAsync(clr, new object?[] { typed });
                if (entity is null) throw new InvalidOperationException($"{clr.Name}({key}) not found");
                return entity;
            }
            finally { _gate.Release(); }
        }

        private static string? GetKey(object e)
        {
            var idProp = e.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase));
            return idProp?.GetValue(e)?.ToString();
        }

        private static string GetDisplay(object e)
        {
            var p = e.GetType().GetProperty("Name") ?? e.GetType().GetProperty("Title");
            var val = p?.GetValue(e)?.ToString();
            return string.IsNullOrWhiteSpace(val) ? e.ToString() ?? "(?)" : val!;
        }

        private object GetSetNonGeneric(Type clr)
        {
            var miNonGeneric = typeof(DbContext).GetMethod("Set", new[] { typeof(Type) });
            if (miNonGeneric != null)
            {
                var set = miNonGeneric.Invoke(_db, new object[] { clr });
                if (set != null) return set;
            }

            var miGeneric = typeof(DbContext).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .First(m => m.Name == "Set" && m.IsGenericMethodDefinition && m.GetParameters().Length == 0);

            var closed = miGeneric.MakeGenericMethod(clr);
            return closed.Invoke(_db, null)!;
        }
    }
}
