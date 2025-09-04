using System;

namespace CMC.Domain.Entities
{
	public class Report : ISoftDeletable, IVersionedEntity
	{
		public Guid Id { get; private set; }

		// Referenzen als Guids (kein Join nötig)
		public Guid? CustomerId { get; private set; }               // optional
		public Guid DefinitionId { get; private set; }              // verweist auf ReportDefinition.Id

		// Zeitraum & Status
		public DateTimeOffset PeriodStart { get; private set; }
		public DateTimeOffset PeriodEnd   { get; private set; }
		public DateTimeOffset GeneratedAt { get; private set; }
		public bool Frozen { get; private set; }

		// Audit
		public DateTimeOffset CreatedAt { get; private set; }
		public DateTimeOffset UpdatedAt { get; private set; }

		// Soft delete
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public string? DeletedBy { get; set; }

		private Report() { }

		public Report(
			Guid definitionId,
			DateTimeOffset periodStartUtc,
			DateTimeOffset periodEndUtc,
			DateTimeOffset generatedAtUtc,
			bool frozen = false,
			Guid? customerId = null,
			DateTimeOffset? createdAtUtc = null)
		{
			if (definitionId == Guid.Empty) throw new ArgumentException("DefinitionId required.", nameof(definitionId));
			if (periodEndUtc < periodStartUtc) throw new ArgumentException("PeriodEnd must be >= PeriodStart");

			Id = Guid.NewGuid();
			DefinitionId = definitionId;
			CustomerId = customerId;

			PeriodStart = periodStartUtc.ToUniversalTime();
			PeriodEnd = periodEndUtc.ToUniversalTime();
			GeneratedAt = generatedAtUtc.ToUniversalTime();
			Frozen = frozen;

			CreatedAt = createdAtUtc?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
			UpdatedAt = CreatedAt;
		}

		public void Freeze()
		{
			Frozen = true;
			Touch();
		}

		public void Unfreeze()
		{
			Frozen = false;
			Touch();
		}

		public void Regenerate(DateTimeOffset generatedAtUtc)
		{
			GeneratedAt = generatedAtUtc.ToUniversalTime();
			Touch();
		}

		public void SetPeriod(DateTimeOffset periodStartUtc, DateTimeOffset periodEndUtc)
		{
			if (periodEndUtc < periodStartUtc) throw new ArgumentException("PeriodEnd must be >= PeriodStart");
			PeriodStart = periodStartUtc.ToUniversalTime();
			PeriodEnd = periodEndUtc.ToUniversalTime();
			Touch();
		}

		public void ReassignCustomer(Guid? customerId)
		{
			CustomerId = customerId;
			Touch();
		}

		public void SetDefinition(Guid definitionId)
		{
			if (definitionId == Guid.Empty) throw new ArgumentException("DefinitionId required.", nameof(definitionId));
			DefinitionId = definitionId;
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
