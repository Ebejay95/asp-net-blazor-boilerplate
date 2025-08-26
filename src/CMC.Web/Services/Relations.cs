using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

			var nav = parentEt.FindNavigation(relationName);
			if (nav is not null)
			{
				var targetEt = nav.TargetEntityType;
				return nav.IsCollection
					? BuildOneToMany(parentType, relationName, targetEt)
					: BuildReference(parentType, relationName, targetEt);
			}

			var skip = parentEt.FindSkipNavigation(relationName);
			if (skip is not null)
			{
				return BuildManyToMany(parentType, relationName, skip.TargetEntityType);
			}

			throw new InvalidOperationException($"Relation '{relationName}' not found on {parentType.Name}");
		}

		public async Task<object> FindParentAsync(Type parentType, object key)
		{
			var set = GetSetNonGeneric(parentType);
			var pk = _db.Model.FindEntityType(parentType)!.FindPrimaryKey()!;
			var keyProp = pk.Properties.Single().PropertyInfo!;
			var typed = Convert.ChangeType(key, keyProp.PropertyType);
			var findAsync = set.GetType().GetMethod("FindAsync", new[] { typeof(object[]) })!;
			var vt = (ValueTask<object?>)findAsync.Invoke(set, new object[] { new object?[] { typed! } })!;
			var entity = await vt;
			if (entity is null) throw new InvalidOperationException($"{parentType.Name}({key}) not found");
			return entity;
		}

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
					var entity = await FindByKey(targetClr, key);
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
					await _db.Entry(parent).Collection(rel).LoadAsync();
					var coll = (IList<object>)_db.Entry(parent).Collection(rel).CurrentValue!;
					foreach (var k in keys)
					{
						var entity = await FindByKey(targetClr, k);
						if (!coll.Contains(entity)) coll.Add(entity);
					}
					await _db.SaveChangesAsync();
				},
				RemoveMany = async (parent, keys) =>
				{
					await _db.Entry(parent).Collection(rel).LoadAsync();
					var coll = (IList<object>)_db.Entry(parent).Collection(rel).CurrentValue!;
					foreach (var k in keys.ToList())
					{
						var entity = await FindByKey(targetClr, k);
						if (coll.Contains(entity)) coll.Remove(entity);
					}
					await _db.SaveChangesAsync();
				}
			};
		}

        private static async Task<List<object>> ToListAsync(object set)
        {
            // DbSet<TEntity> implementiert IQueryable<TEntity>
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
			var set = GetSetNonGeneric(clr);
			var pk = _db.Model.FindEntityType(clr)!.FindPrimaryKey()!;
			var keyProp = pk.Properties.Single().PropertyInfo!;
			var typed = Convert.ChangeType(key, keyProp.PropertyType);
			var findAsync = set.GetType().GetMethod("FindAsync", new[] { typeof(object[]) })!;
			var vt = (ValueTask<object?>)findAsync.Invoke(set, new object[] { new object?[] { typed! } })!;
			var e = await vt;
			if (e is null) throw new InvalidOperationException($"{clr.Name}({key}) not found");
			return e;
		}

		private static string? GetKey(object e)
		{
			var idProp = e.GetType().GetProperties().FirstOrDefault(p => string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase));
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
            // 1) Versuche non-generic DbContext.Set(Type) (falls vorhanden)
            var miNonGeneric = typeof(DbContext).GetMethod("Set", new[] { typeof(Type) });
            if (miNonGeneric != null)
            {
                var set = miNonGeneric.Invoke(_db, new object[] { clr });
                if (set != null) return set;
            }

            // 2) Fallback: generische Definition DbContext.Set<TEntity>() schließen
            var miGeneric = typeof(DbContext).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .First(m => m.Name == "Set" && m.IsGenericMethodDefinition && m.GetParameters().Length == 0);

            var closed = miGeneric.MakeGenericMethod(clr);
            return closed.Invoke(_db, null)!;
        }
	}
}
