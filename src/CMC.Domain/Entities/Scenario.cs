using System;

namespace CMC.Domain.Entities
{
	public class Scenario : ISoftDeletable
	{
		public Guid Id { get; private set; }

		// Beziehungen
		public Guid CustomerId { get; private set; }
		public virtual Customer? Customer { get; private set; }

		public Guid LibraryScenarioId { get; private set; }
		public virtual LibraryScenario? LibraryScenario { get; private set; }

		// Frei editierbare Felder (vom Template kopiert, kundenspezifisch)
		public string Name { get; private set; } = string.Empty;
		public decimal AnnualFrequency { get; private set; }
		public decimal ImpactPctRevenue { get; private set; }
		public string Tags { get; private set; } = string.Empty;

		// Audit
		public DateTimeOffset CreatedAt { get; private set; }
		public DateTimeOffset UpdatedAt { get; private set; }

		// Soft Delete
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public string? DeletedBy { get; set; }

		private Scenario() { }

		public Scenario(
			Guid customerId,
			Guid libraryScenarioId,
			string name,
			decimal annualFrequency,
			decimal impactPctRevenue,
			string? tags = null,
			DateTimeOffset? createdAtUtc = null)
		{
			if (customerId == Guid.Empty) throw new ArgumentException("CustomerId required.", nameof(customerId));
			if (libraryScenarioId == Guid.Empty) throw new ArgumentException("LibraryScenarioId required.", nameof(libraryScenarioId));
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));

			Id = Guid.NewGuid();
			CustomerId = customerId;
			LibraryScenarioId = libraryScenarioId;

			Name = name.Trim();
			AnnualFrequency = annualFrequency;
			ImpactPctRevenue = impactPctRevenue;
			Tags = (tags ?? string.Empty).Trim();

			CreatedAt = createdAtUtc?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
			UpdatedAt = CreatedAt;
		}

        public static Scenario FromLibrary(Guid customerId, LibraryScenario lib, DateTimeOffset? createdAtUtc = null)
        {
            if (customerId == Guid.Empty) throw new ArgumentException("CustomerId required.", nameof(customerId));
            if (lib == null) throw new ArgumentNullException(nameof(lib));

            return new Scenario(
                customerId: customerId,
                libraryScenarioId: lib.Id,
                name: lib.Name,
                annualFrequency: lib.AnnualFrequency,
                impactPctRevenue: lib.ImpactPctRevenue,
                tags: lib.Tags,
                createdAtUtc: createdAtUtc
            );
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

		public void ReassignCustomer(Guid customerId)
		{
			if (customerId == Guid.Empty) throw new ArgumentException("CustomerId required.", nameof(customerId));
			CustomerId = customerId;
			Touch();
		}

		public void SetLibraryScenario(Guid libraryScenarioId)
		{
			if (libraryScenarioId == Guid.Empty) throw new ArgumentException("LibraryScenarioId required.", nameof(libraryScenarioId));
			LibraryScenarioId = libraryScenarioId;
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
