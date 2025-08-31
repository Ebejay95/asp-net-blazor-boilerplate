using System;
using System.Collections.Generic;
using System.Linq;

namespace CMC.Domain.Entities
{
	public class LibraryControl : ISoftDeletable
	{
		public Guid Id { get; private set; }
		public string Name { get; private set; } = string.Empty;

		public decimal CapexEur { get; private set; }
		public decimal OpexYearEur { get; private set; }

		public int InternalDays { get; private set; }
		public int ExternalDays { get; private set; }
		public int TotalDays { get; private set; }

		// Join-Links (M:N)
		public virtual ICollection<LibraryControlTag> TagLinks { get; private set; } = new List<LibraryControlTag>();
		public virtual ICollection<LibraryControlIndustry> IndustryLinks { get; private set; } = new List<LibraryControlIndustry>();

		// (bereits vorhanden)
		public virtual ICollection<LibraryControlScenario> ScenarioLinks { get; private set; } = new List<LibraryControlScenario>();
		public virtual ICollection<LibraryControlFramework> FrameworkLinks { get; private set; } = new List<LibraryControlFramework>();

		public DateTimeOffset CreatedAt { get; private set; }
		public DateTimeOffset UpdatedAt { get; private set; }

		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public string? DeletedBy { get; set; }

		private LibraryControl() { }

		public LibraryControl(
			string name,
			decimal capexEur,
			decimal opexYearEur,
			int internalDays,
			int externalDays,
			IEnumerable<Guid>? tagIds = null,
			IEnumerable<Guid>? industryIds = null,
			int? totalDays = null,
			DateTimeOffset? createdAtUtc = null)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
			if (capexEur < 0) throw new ArgumentException("Capex must be >= 0", nameof(capexEur));
			if (opexYearEur < 0) throw new ArgumentException("Opex must be >= 0", nameof(opexYearEur));
			if (internalDays < 0 || externalDays < 0) throw new ArgumentException("Days must be >= 0");

			Id = Guid.NewGuid();
			Name = name.Trim();
			CapexEur = capexEur;
			OpexYearEur = opexYearEur;
			InternalDays = internalDays;
			ExternalDays = externalDays;
			TotalDays = totalDays ?? (internalDays + externalDays);

			CreatedAt = createdAtUtc?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
			UpdatedAt = CreatedAt;

			SetTags(tagIds);
			SetIndustries(industryIds);
		}

		public void UpdateEffort(int internalDays, int externalDays, int? totalDays = null)
		{
            if (internalDays < 0 || externalDays < 0) throw new ArgumentException("Days must be >= 0");
            InternalDays = internalDays;
            ExternalDays = externalDays;
            TotalDays = totalDays ?? (internalDays + externalDays);
            Touch();
		}

		public void UpdateCosts(decimal capexEur, decimal opexYearEur)
		{
			if (capexEur < 0 || opexYearEur < 0) throw new ArgumentException("Costs must be >= 0");
			CapexEur = capexEur;
			OpexYearEur = opexYearEur;
			Touch();
		}

		public void Rename(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
			Name = name.Trim();
			Touch();
		}

		public void SetTags(IEnumerable<Guid>? tagIds)
		{
			var target = new HashSet<Guid>((tagIds ?? Enumerable.Empty<Guid>()).Where(x => x != Guid.Empty));
			var toRemove = TagLinks.Where(l => !target.Contains(l.TagId)).ToList();
			foreach (var r in toRemove) TagLinks.Remove(r);

			var existing = new HashSet<Guid>(TagLinks.Select(l => l.TagId));
			foreach (var id in target.Except(existing))
				TagLinks.Add(new LibraryControlTag(Id, id));

			Touch();
		}

		public void SetIndustries(IEnumerable<Guid>? industryIds)
		{
			var target = new HashSet<Guid>((industryIds ?? Enumerable.Empty<Guid>()).Where(x => x != Guid.Empty));
			var toRemove = IndustryLinks.Where(l => !target.Contains(l.IndustryId)).ToList();
			foreach (var r in toRemove) IndustryLinks.Remove(r);

			var existing = new HashSet<Guid>(IndustryLinks.Select(l => l.IndustryId));
			foreach (var id in target.Except(existing))
				IndustryLinks.Add(new LibraryControlIndustry(Id, id));

			Touch();
		}

		public IReadOnlyList<Guid> GetTagIds() => TagLinks.Select(x => x.TagId).ToArray();
		public IReadOnlyList<Guid> GetIndustryIds() => IndustryLinks.Select(x => x.IndustryId).ToArray();

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
