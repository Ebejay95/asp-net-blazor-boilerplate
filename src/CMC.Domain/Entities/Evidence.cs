using System;
using System.Collections.Generic;

namespace CMC.Domain.Entities
{
    public class Evidence : ISoftDeletable, IVersionedEntity
    {
        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }

        public string Source { get; private set; } = string.Empty;
        public string Location { get; private set; } = string.Empty;

        public DateTimeOffset CollectedAt { get; private set; }
        public DateTimeOffset? ValidUntil { get; private set; }

        public string HashSha256 { get; private set; } = string.Empty;
        public string Confidentiality { get; private set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        // Gegenrichtung zu Control.Evidence (1:n)
        public virtual ICollection<Control> Controls { get; private set; } = new List<Control>();

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
            EnsureValidityRange();

            HashSha256 = NormalizeHash(hashSha256);
            Confidentiality = (confidentiality ?? string.Empty).Trim();

            CreatedAt = (createdAtUtc ?? DateTimeOffset.UtcNow).ToUniversalTime();
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
            EnsureValidityRange();
            Touch();
        }

        public void SetHashSha256(string? hashSha256)
        {
            HashSha256 = NormalizeHash(hashSha256);
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

        private void EnsureValidityRange()
        {
            if (ValidUntil.HasValue && ValidUntil.Value < CollectedAt)
                throw new ArgumentException("ValidUntil cannot be before CollectedAt.");
        }

        private static string NormalizeHash(string? input)
        {
            var s = (input ?? string.Empty).Trim();
            return s.Length == 0 ? string.Empty : s.ToLowerInvariant();
        }

        private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
    }
}
