using System;
using System.Collections.Generic;
using System.Linq;

namespace CMC.Domain.Entities
{
	public class Framework : IVersionedEntity, ISoftDeletable
	{
		public Guid Id { get; private set; }
		public string Name { get; private set; } = string.Empty;
		public string Version { get; private set; } = string.Empty;

		// M:N via Link-Entity
		public virtual ICollection<FrameworkIndustry> IndustryLinks { get; private set; } = new List<FrameworkIndustry>();

		// bleibt wie gehabt
		public virtual ICollection<LibraryControlFramework> ControlLinks { get; private set; } = new List<LibraryControlFramework>();

		public DateTimeOffset CreatedAt { get; private set; }
		public DateTimeOffset UpdatedAt { get; private set; }

		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public string? DeletedBy { get; set; }

		private Framework() { }

		public Framework(string name, string? version = null, IEnumerable<Guid>? industryIds = null, DateTimeOffset? createdAtUtc = null)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Name required.", nameof(name));

			Id = Guid.NewGuid();
			Name = name.Trim();
			Version = (version ?? string.Empty).Trim();

			CreatedAt = createdAtUtc?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
			UpdatedAt = CreatedAt;

			SetIndustries(industryIds);
		}

		public void Rename(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
			Name = name.Trim();
			Touch();
		}

		public void SetVersion(string? version)
		{
			Version = (version ?? string.Empty).Trim();
			Touch();
		}

		public void SetIndustries(IEnumerable<Guid>? industryIds)
		{
			var target = industryIds?
				.Where(x => x != Guid.Empty)
				.Distinct()
				.ToHashSet() ?? new HashSet<Guid>();

			var toRemove = IndustryLinks.Where(l => !target.Contains(l.IndustryId)).ToList();
			foreach (var r in toRemove) IndustryLinks.Remove(r);

			var existing = IndustryLinks.Select(l => l.IndustryId).ToHashSet();
			foreach (var id in target)
			{
				if (!existing.Contains(id))
					IndustryLinks.Add(new FrameworkIndustry(Id, id));
			}

			Touch();
		}

		public IReadOnlyList<Guid> GetIndustryIds() => IndustryLinks.Select(l => l.IndustryId).Distinct().ToArray();

		public void Delete(string? deletedBy = null)
		{
			if (!IsDeleted)
			{
				IsDeleted = true;
				DeletedAt = DateTimeOffset.UtcNow;
				DeletedBy = deletedBy;
			}
		}

		public void Restore()
		{
			if (IsDeleted)
			{
				IsDeleted = false;
				DeletedAt = null;
				DeletedBy = null;
				Touch();
			}
		}

		private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
	}
}
