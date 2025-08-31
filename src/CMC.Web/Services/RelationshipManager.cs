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
    /// Ermittelt Relations-Metadaten aus dem EF Core Modell und stellt generische Loader/Mutatoren
    /// für Reference, 1:n und M:n bereit (Skip-Navigation ODER explizite Join-Entity).
    ///
    /// Nutzt wenn möglich Convention-basierte Domain-Methoden:
    /// - Add{RelationSuffix}(IEnumerable&lt;Guid&gt;)
    /// - Remove{RelationSuffix}(IEnumerable&lt;Guid&gt;)
    /// - Get{RelationSuffix}Keys() : IEnumerable&lt;Guid&gt; (optional)
    /// </summary>
    public sealed class RelationshipManager<TDbContext> : IRelationshipManager where TDbContext : DbContext
    {
        private readonly TDbContext _db;

        public RelationshipManager(TDbContext db) { _db = db; }

        public RelationDescriptor GetDescriptor<TParent>(string relationName) where TParent : class
            => GetDescriptor(typeof(TParent), relationName);

        public RelationDescriptor GetDescriptor(Type parentType, string relationName)
        {
            var model = _db.Model;
            var parentEt = model.FindEntityType(parentType)
                ?? throw new InvalidOperationException($"EntityType not found: {parentType.Name}");

            // 1) Normale Navigation (Reference oder 1:n)
            var nav = parentEt.FindNavigation(relationName);
            if (nav is not null)
            {
                var targetEt = nav.TargetEntityType;

                // Explizite Join-Collection als M:N erkennen (Collection of JoinEntity)
                if (nav.IsCollection && LooksLikeExplicitJoinCollection(parentEt, nav.TargetEntityType))
                {
                    var fks = nav.TargetEntityType.GetForeignKeys().ToList();
                    var fkToParent = fks.FirstOrDefault(fk => fk.PrincipalEntityType == parentEt);
                    var fkToTarget = fks.FirstOrDefault(fk => fk.PrincipalEntityType != parentEt)
                                     ?? throw new InvalidOperationException($"Join '{relationName}' auf {parentType.Name} hat keine erwarteten FKs.");
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

            // 2) Skip-Navigation (echtes Many-to-Many ohne explizite Join-Entity)
            var skip = parentEt.FindSkipNavigation(relationName);
            if (skip is not null)
            {
                return BuildManyToMany(parentType, relationName, skip.TargetEntityType);
            }

            // 3) Fallback: Explizite Join-Collection über Name (Collection-Navi mit 2 FKs suchen)
            var joinNav = parentEt
                .GetNavigations()
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
            var et = _db.Model.FindEntityType(parentType)
                    ?? throw new InvalidOperationException($"EntityType not found: {parentType.Name}");

            var pkProp = et.FindPrimaryKey()!.Properties.Single();
            var pkClr  = pkProp.ClrType;

            // Key richtig typisieren (Guid, int, …)
            object? typed = key;
            if (key is string s)
            {
                typed = pkClr == typeof(Guid) || pkClr == typeof(Guid?)
                    ? Guid.Parse(s)
                    : Convert.ChangeType(s, Nullable.GetUnderlyingType(pkClr) ?? pkClr);
            }

            var entity = await _db.FindAsync(parentType, new object?[] { typed! }); // ✅ ValueTask<object?>
            if (entity is null) throw new InvalidOperationException($"{parentType.Name}({key}) not found");
            return entity;
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
                    var set = GetSetNonGeneric(targetClr);
                    var list = await ToListAsync(set);
                    return list.Select(e => new RelationOption(GetDisplay(e), GetKey(e)!));
                },

                LoadCurrentKeys = async parent =>
                {
                    await _db.Entry(parent).Reference(rel).LoadAsync();
                    var obj = _db.Entry(parent).Reference(rel).CurrentValue;
                    return obj is null ? Array.Empty<string>() : new[] { GetKey(obj)! };
                },

                SetReference = async (parent, key) =>
                {
                    var entity = string.IsNullOrWhiteSpace(key) ? null : await FindByKey(targetClr, key);
                    _db.Entry(parent).Reference(rel).CurrentValue = entity;
                    await _db.SaveChangesAsync();
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

                // Für 1:n gibt es im Parent-Editor typischerweise keine Auswahl
                LoadOptions = () => Task.FromResult<IEnumerable<RelationOption>>(Array.Empty<RelationOption>()),

                LoadCurrentKeys = async parent =>
                {
                    await _db.Entry(parent).Collection(rel).LoadAsync();
                    var children = (IEnumerable<object>)(_db.Entry(parent).Collection(rel).CurrentValue ?? Array.Empty<object>());
                    return children.Select(GetKey!).Where(k => k is not null)!;
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
                    var set = GetSetNonGeneric(targetClr);
                    var list = await ToListAsync(set);
                    return list.Select(e => new RelationOption(GetDisplay(e), GetKey(e)!));
                },

                LoadCurrentKeys = async parent =>
                {
                    await _db.Entry(parent).Collection(rel).LoadAsync();
                    var curr = (IEnumerable<object>)(_db.Entry(parent).Collection(rel).CurrentValue ?? Array.Empty<object>());
                    return curr.Select(GetKey!).Where(k => k is not null)!;
                },

                AddMany = async (parent, keys) =>
                {
                    // Domain-Methoden versuchen (Convention-based)
                    if (await TryUseDomainMethodAsync(parent, rel, "Add", keys))
                        return;

                    // Fallback: EF Collection Manipulation
                    await _db.Entry(parent).Collection(rel).LoadAsync();
                    var coll = (IList)_db.Entry(parent).Collection(rel).CurrentValue!;

                    foreach (var k in keys)
                    {
                        var entity = await FindByKey(targetClr, k);
                        if (!coll.Contains(entity))
                            coll.Add(entity);
                    }

                    await _db.SaveChangesAsync();
                },

                RemoveMany = async (parent, keys) =>
                {
                    // Domain-Methoden versuchen
                    if (await TryUseDomainMethodAsync(parent, rel, "Remove", keys))
                        return;

                    // Fallback: EF Collection Manipulation
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
            };
        }

        /// <summary>
        /// M:N unter Verwendung einer *expliziten* Join-Entity (z. B. CustomerIndustries) behandeln.
        /// Die Navigation am Parent ist hier eine Collection der Join-Entity.
        /// </summary>
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

            var fkParentProp = fkToParent.Properties.Single().PropertyInfo!; // z.B. CustomerId
            var fkTargetProp = fkToTarget.Properties.Single().PropertyInfo!; // z.B. IndustryId
            var parentPkProp = _db.Model.FindEntityType(parentType)!.FindPrimaryKey()!.Properties.Single().PropertyInfo!;

            return new RelationDescriptor
            {
                Name = joinCollectionName,
                Kind = RelationKind.ManyToMany,
                ParentType = parentType,
                TargetType = targetClr,

                LoadOptions = async () =>
                {
                    var set = GetSetNonGeneric(targetClr);
                    var list = await ToListAsync(set);
                    return list.Select(e => new RelationOption(GetDisplay(e), GetKey(e)!));
                },

                LoadCurrentKeys = async parent =>
                {
                    // Optionaler Domain-Reader (Get{Suffix}Keys)
                    var domainKeys = TryGetDomainKeys(parent, joinCollectionName);
                    if (domainKeys != null)
                        return domainKeys.Select(k => k.ToString()).ToArray();

                    // Fallback: EF-Join-Collection lesen und FK zum Target extrahieren
                    await _db.Entry(parent).Collection(joinCollectionName).LoadAsync();
                    var coll = (IEnumerable<object>)(_db.Entry(parent).Collection(joinCollectionName).CurrentValue ?? Array.Empty<object>());

                    return coll
                        .Select(j => fkTargetProp.GetValue(j)?.ToString())
                        .Where(s => !string.IsNullOrWhiteSpace(s))!
                        .ToArray()!;
                },

                AddMany = async (parent, keys) =>
                {
                    // Domain-Methoden versuchen (Add{Suffix})
                    if (await TryUseDomainMethodAsync(parent, joinCollectionName, "Add", keys))
                        return;

                    // Fallback: Join-Entity Instanzen erzeugen
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
                },

                RemoveMany = async (parent, keys) =>
                {
                    // Domain-Methoden versuchen (Remove{Suffix})
                    if (await TryUseDomainMethodAsync(parent, joinCollectionName, "Remove", keys))
                        return;

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
            };
        }

        // ------------------------ Domain Convention Helpers ------------------------

        /// <summary>
        /// Versucht Convention-basierte Domain-Methoden:
        /// - Add{RelationSuffix}(IEnumerable&lt;Guid&gt;)
        /// - Remove{RelationSuffix}(IEnumerable&lt;Guid&gt;)
        ///
        /// Convention: CustomerIndustries -> AddIndustries / RemoveIndustries
        /// </summary>
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

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"🎯 Using domain method: {parent.GetType().Name}.{methodName}() with {guids.Length} keys");
                    Console.ResetColor();

                    method.Invoke(parent, new object[] { guids });
                    await _db.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ Domain method {methodName} failed: {ex.Message}");
                    Console.ResetColor();
                }
            }

            return false;
        }

        /// <summary>
        /// Optionaler Domain-Leser:
        /// - Get{RelationSuffix}Keys() : IEnumerable&lt;Guid&gt;
        /// </summary>
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
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"🔑 Using domain method: {parent.GetType().Name}.{methodName}()");
                        Console.ResetColor();
                        return keys;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ Domain method {methodName} failed: {ex.Message}");
                    Console.ResetColor();
                }
            }

            return null;
        }

        /// <summary>
        /// Leitet aus dem Relations-Namen den Suffix für Domain-Methoden ab.
        /// z.B. "CustomerIndustries" -> "Industries"
        /// </summary>
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

            // Allgemeine Convention: Parent-Präfix entfernen (CustomerX -> X)
            var commonParents = new[] { "Customer", "User", "Order", "Product", "Company" };
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
            var et    = _db.Model.FindEntityType(clr)!;
            var pk    = et.FindPrimaryKey()!.Properties.Single();
            var pkClr = pk.ClrType;

            object typed = (pkClr == typeof(Guid) || pkClr == typeof(Guid?))
                ? Guid.Parse(key)
                : Convert.ChangeType(key, Nullable.GetUnderlyingType(pkClr) ?? pkClr)!;

            var entity = await _db.FindAsync(clr, new object?[] { typed }); // ✅ ValueTask<object?>
            if (entity is null) throw new InvalidOperationException($"{clr.Name}({key}) not found");
            return entity;
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
            // 1) DbContext.Set(Type) (non-generic)
            var miNonGeneric = typeof(DbContext).GetMethod("Set", new[] { typeof(Type) });
            if (miNonGeneric != null)
            {
                var set = miNonGeneric.Invoke(_db, new object[] { clr });
                if (set != null) return set;
            }

            // 2) Fallback: DbContext.Set<TEntity>()
            var miGeneric = typeof(DbContext).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .First(m => m.Name == "Set" && m.IsGenericMethodDefinition && m.GetParameters().Length == 0);

            var closed = miGeneric.MakeGenericMethod(clr);
            return closed.Invoke(_db, null)!;
        }
    }
}
