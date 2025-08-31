using System;
using System.Collections.Generic;
using System.Linq;

namespace CMC.Domain.Entities
{
    public class Scenario : ISoftDeletable, IVersionedEntity
    {
        public Guid Id { get; private set; }

        public Guid CustomerId { get; private set; }
        public virtual Customer? Customer { get; private set; }

        public Guid LibraryScenarioId { get; private set; }
        public virtual LibraryScenario? LibraryScenario { get; private set; }

        public string Name { get; private set; } = string.Empty;
        public decimal AnnualFrequency { get; private set; }
        public decimal ImpactPctRevenue { get; private set; }

        // Tags (Join-Entity)
        public virtual ICollection<ScenarioTag> TagLinks { get; private set; } = new List<ScenarioTag>();

        // M:N ↔ Control
        public virtual ICollection<ControlScenario> ControlLinks { get; private set; } = new List<ControlScenario>();

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

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
            IEnumerable<Guid>? tagIds = null,
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

            CreatedAt = createdAtUtc?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;

            SetTags(tagIds);
        }

        public static Scenario FromLibrary(Guid customerId, LibraryScenario lib, DateTimeOffset? createdAtUtc = null)
            => new Scenario(customerId, lib.Id, lib.Name, lib.AnnualFrequency, lib.ImpactPctRevenue, lib.GetTagIds(), createdAtUtc);

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
            Name = name.Trim(); Touch();
        }

        public void SetAnnualFrequency(decimal v) { AnnualFrequency = v; Touch(); }
        public void SetImpactPctRevenue(decimal v) { ImpactPctRevenue = v; Touch(); }

        public void SetTags(IEnumerable<Guid>? tagIds)
        {
            var target = new HashSet<Guid>((tagIds ?? Enumerable.Empty<Guid>()).Where(x => x != Guid.Empty));
            var toRemove = TagLinks.Where(l => !target.Contains(l.TagId)).ToList();
            foreach (var r in toRemove) TagLinks.Remove(r);

            var existing = TagLinks.Select(l => l.TagId).ToHashSet();
            foreach (var id in target.Except(existing)) TagLinks.Add(new ScenarioTag(Id, id));

            Touch();
        }

        public IReadOnlyList<Guid> GetTagIds() => TagLinks.Select(x => x.TagId).ToArray();

        public void ReassignCustomer(Guid customerId)
        {
            if (customerId == Guid.Empty) throw new ArgumentException("CustomerId required.", nameof(customerId));
            CustomerId = customerId; Touch();
        }

        public void SetLibraryScenario(Guid libraryScenarioId)
        {
            if (libraryScenarioId == Guid.Empty) throw new ArgumentException("LibraryScenarioId required.", nameof(libraryScenarioId));
            LibraryScenarioId = libraryScenarioId; Touch();
        }

        public void Delete(string? deletedBy = null)
        {
            if (!IsDeleted)
            {
                IsDeleted = true; DeletedAt = DateTimeOffset.UtcNow; DeletedBy = deletedBy;
            }
        }

        public void Restore()
        {
            if (IsDeleted)
            {
                IsDeleted = false; DeletedAt = null; DeletedBy = null; Touch();
            }
        }

        private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
    }
}
