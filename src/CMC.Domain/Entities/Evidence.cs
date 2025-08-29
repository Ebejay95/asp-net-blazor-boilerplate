// Entities/Evidence.cs
using System;

namespace CMC.Domain.Entities
{
	public class Evidence : ISoftDeletable
	{
		public Guid Id { get; private set; }

		// belongs to exactly one customer
		public Guid CustomerId { get; private set; }
		public virtual Customer? Customer { get; private set; }

		// metadata
		public string Source { get; private set; } = string.Empty;          // e.g. "file", "api", "manual"
		public string Location { get; private set; } = string.Empty;        // e.g. s3://...; may be empty
		public DateTimeOffset CollectedAt { get; private set; }             // collected_at (UTC)
		public DateTimeOffset? ValidUntil { get; private set; }             // valid_until (UTC, optional)
		public string HashSha256 { get; private set; } = string.Empty;      // optional; may be empty
		public string Confidentiality { get; private set; } = string.Empty; // e.g. "internal"; optional

		// audit
		public DateTimeOffset CreatedAt { get; private set; }
		public DateTimeOffset UpdatedAt { get; private set; }

		// soft delete
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public string? DeletedBy { get; set; }

		private Evidence() { }

		public Evidence(
			Guid customerId,
			string source,
			DateTimeOffset collectedAtUtc,
			string? location = null,
			DateTimeOffset? validUntilUtc = null,
			string? hashSha256 = null,
			string? confidentiality = null,
			DateTimeOffset? createdAtUtc = null)
		{
			if (customerId == Guid.Empty) throw new ArgumentException("CustomerId required.", nameof(customerId));
			if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("Source required.", nameof(source));

			Id = Guid.NewGuid();

			CustomerId = customerId;
			Source = source.Trim();
			Location = (location ?? string.Empty).Trim();
			CollectedAt = collectedAtUtc.ToUniversalTime();
			ValidUntil = validUntilUtc?.ToUniversalTime();
			HashSha256 = (hashSha256 ?? string.Empty).Trim();
			Confidentiality = (confidentiality ?? string.Empty).Trim();

			CreatedAt = createdAtUtc?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
			UpdatedAt = CreatedAt;
		}

		public void ReassignCustomer(Guid customerId)
		{
			if (customerId == Guid.Empty) throw new ArgumentException("CustomerId required.", nameof(customerId));
			CustomerId = customerId;
			Touch();
		}

		public void UpdateSource(string source)
		{
			if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("Source required.", nameof(source));
			Source = source.Trim();
			Touch();
		}

		public void SetLocation(string? location)
		{
			Location = (location ?? string.Empty).Trim();
			Touch();
		}

		public void SetValidity(DateTimeOffset? validUntilUtc)
		{
			ValidUntil = validUntilUtc?.ToUniversalTime();
			Touch();
		}

		public void SetHashSha256(string? hashSha256)
		{
			HashSha256 = (hashSha256 ?? string.Empty).Trim();
			Touch();
		}

		public void SetConfidentiality(string? confidentiality)
		{
			Confidentiality = (confidentiality ?? string.Empty).Trim();
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
