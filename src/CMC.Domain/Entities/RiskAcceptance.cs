// Entities/RiskAcceptance.cs
using System;

namespace CMC.Domain.Entities
{
	public class RiskAcceptance : ISoftDeletable
	{
		public Guid Id { get; private set; }

		// Referenzen als Guids (kein Join)
		public Guid CustomerId { get; private set; }    // z.B. verweist auf Customer.Id
		public Guid ControlId { get; private set; }     // z.B. verweist auf Control.Id

		public string Reason { get; private set; } = string.Empty;         // Begründung
		public string RiskAcceptedBy { get; private set; } = string.Empty; // z.B. "CEO"
		public DateTimeOffset ExpiresAt { get; private set; }              // Ablaufdatum (UTC)

		// Audit
		public DateTimeOffset CreatedAt { get; private set; }
		public DateTimeOffset UpdatedAt { get; private set; }

		// Soft delete
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public string? DeletedBy { get; set; }

		private RiskAcceptance() { }

		public RiskAcceptance(
			Guid customerId,
			Guid controlId,
			string reason,
			string riskAcceptedBy,
			DateTimeOffset expiresAtUtc,
			DateTimeOffset? createdAtUtc = null)
		{
			if (customerId == Guid.Empty) throw new ArgumentException("CustomerId required.", nameof(customerId));
			if (controlId == Guid.Empty) throw new ArgumentException("ControlId required.", nameof(controlId));
			if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Reason required.", nameof(reason));
			if (string.IsNullOrWhiteSpace(riskAcceptedBy)) throw new ArgumentException("RiskAcceptedBy required.", nameof(riskAcceptedBy));

			Id = Guid.NewGuid();
			CustomerId = customerId;
			ControlId = controlId;
			Reason = reason.Trim();
			RiskAcceptedBy = riskAcceptedBy.Trim();
			ExpiresAt = expiresAtUtc.ToUniversalTime();

			CreatedAt = createdAtUtc?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
			UpdatedAt = CreatedAt;
		}

		public void UpdateReason(string reason)
		{
			if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Reason required.", nameof(reason));
			Reason = reason.Trim();
			Touch();
		}

		public void UpdateRiskAcceptedBy(string riskAcceptedBy)
		{
			if (string.IsNullOrWhiteSpace(riskAcceptedBy)) throw new ArgumentException("RiskAcceptedBy required.", nameof(riskAcceptedBy));
			RiskAcceptedBy = riskAcceptedBy.Trim();
			Touch();
		}

		public void SetExpiry(DateTimeOffset expiresAtUtc)
		{
			ExpiresAt = expiresAtUtc.ToUniversalTime();
			Touch();
		}

		public void SetRefs(Guid customerId, Guid controlId)
		{
			if (customerId == Guid.Empty) throw new ArgumentException("CustomerId required.", nameof(customerId));
			if (controlId == Guid.Empty) throw new ArgumentException("ControlId required.", nameof(controlId));
			CustomerId = customerId;
			ControlId = controlId;
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
