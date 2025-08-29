using System;
using System.Collections.Generic;

namespace CMC.Domain.Entities
{
	public class LibraryScenario : ISoftDeletable
	{
		public Guid Id { get; private set; }
		public string Name { get; private set; } = string.Empty;

		public decimal AnnualFrequency { get; private set; }
		public decimal ImpactPctRevenue { get; private set; }

		public string Tags { get; private set; } = string.Empty;

		public DateTimeOffset CreatedAt { get; private set; }
		public DateTimeOffset UpdatedAt { get; private set; }

		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public string? DeletedBy { get; set; }

		public virtual ICollection<LibraryControlScenario> ControlLinks { get; private set; } = new List<LibraryControlScenario>();
		public virtual ICollection<LibraryScenarioIndustry> IndustryLinks { get; private set; } = new List<LibraryScenarioIndustry>();

		private LibraryScenario() { }

		public LibraryScenario(
			string name,
			decimal annualFrequency,
			decimal impactPctRevenue,
			string? tags = null,
			DateTimeOffset? createdAtUtc = null)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));

			Id = Guid.NewGuid();
			Name = name.Trim();
			AnnualFrequency = annualFrequency;
			ImpactPctRevenue = impactPctRevenue;
			Tags = (tags ?? string.Empty).Trim();

			CreatedAt = createdAtUtc?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
			UpdatedAt = CreatedAt;
		}

		public void Rename(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
			Name = name.Trim();
			Touch();
		}

		public void SetAnnualFrequency(decimal value)
		{
			AnnualFrequency = value;
			Touch();
		}

		public void SetImpactPctRevenue(decimal value)
		{
			ImpactPctRevenue = value;
			Touch();
		}

		public void SetTags(string? tags)
		{
			Tags = (tags ?? string.Empty).Trim();
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
