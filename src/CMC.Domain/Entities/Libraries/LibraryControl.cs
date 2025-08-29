using System;
using System.Collections.Generic;

namespace CMC.Domain.Entities
{
	public class LibraryControl : ISoftDeletable
	{
		public Guid Id { get; private set; }
		public string Name { get; private set; } = string.Empty;
		public string Tag { get; private set; } = string.Empty;

		public decimal CapexEur { get; private set; }
		public decimal OpexYearEur { get; private set; }

		public int InternalDays { get; private set; }
		public int ExternalDays { get; private set; }
		public int TotalDays { get; private set; }

		public DateTimeOffset CreatedAt { get; private set; }
		public DateTimeOffset UpdatedAt { get; private set; }

		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public string? DeletedBy { get; set; }

		public virtual ICollection<LibraryControlScenario> ScenarioLinks { get; private set; } = new List<LibraryControlScenario>();
		public virtual ICollection<LibraryControlFramework> FrameworkLinks { get; private set; } = new List<LibraryControlFramework>();
		public virtual ICollection<LibraryControlIndustry> IndustryLinks { get; private set; } = new List<LibraryControlIndustry>();

		private LibraryControl() { }

		public LibraryControl(
			string name,
			string tag,
			decimal capexEur,
			decimal opexYearEur,
			int internalDays,
			int externalDays,
			int? totalDays = null,
			DateTimeOffset? createdAtUtc = null)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
			if (string.IsNullOrWhiteSpace(tag)) throw new ArgumentException("Tag required.", nameof(tag));
			if (capexEur < 0) throw new ArgumentException("Capex must be >= 0", nameof(capexEur));
			if (opexYearEur < 0) throw new ArgumentException("Opex must be >= 0", nameof(opexYearEur));
			if (internalDays < 0 || externalDays < 0) throw new ArgumentException("Days must be >= 0");

			Id = Guid.NewGuid();
			Name = name.Trim();
			Tag = tag.Trim();
			CapexEur = capexEur;
			OpexYearEur = opexYearEur;
			InternalDays = internalDays;
			ExternalDays = externalDays;
			TotalDays = totalDays ?? (internalDays + externalDays);

			CreatedAt = createdAtUtc?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
			UpdatedAt = CreatedAt;
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

		public void Retag(string tag)
		{
			if (string.IsNullOrWhiteSpace(tag)) throw new ArgumentException("Tag required.", nameof(tag));
			Tag = tag.Trim();
			Touch();
		}

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
