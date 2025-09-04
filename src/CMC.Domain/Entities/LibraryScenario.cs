using System;
using System.Collections.Generic;
using System.Linq;

namespace CMC.Domain.Entities
{
    public class LibraryScenario : ISoftDeletable, IVersionedEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public decimal AnnualFrequency { get; private set; }
        public decimal ImpactPctRevenue { get; private set; }

        // Tags & Industries über Join-Entities
        public virtual ICollection<LibraryScenarioTag> TagLinks { get; private set; } = new List<LibraryScenarioTag>();
        public virtual ICollection<LibraryScenarioIndustry> IndustryLinks { get; private set; } = new List<LibraryScenarioIndustry>();

        // Rück-Navigation zu LibraryControl über Join (beidseitig)
        public virtual ICollection<LibraryControlScenario> ControlLinks { get; private set; } = new List<LibraryControlScenario>();

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        private LibraryScenario() { }

        public LibraryScenario(
            string name,
            decimal annualFrequency,
            decimal impactPctRevenue,
            IEnumerable<Guid>? tagIds = null,
            IEnumerable<Guid>? industryIds = null,
            DateTimeOffset? createdAtUtc = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));

            Id = Guid.NewGuid();
            Name = name.Trim();
            AnnualFrequency = annualFrequency;
            ImpactPctRevenue = impactPctRevenue;

            CreatedAt = createdAtUtc?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;

            SetTags(tagIds);
            SetIndustries(industryIds);
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

        public void SetTags(IEnumerable<Guid>? tagIds)
        {
            var target = new HashSet<Guid>((tagIds ?? Enumerable.Empty<Guid>()).Where(x => x != Guid.Empty));

            var toRemove = TagLinks.Where(l => !target.Contains(l.TagId)).ToList();
            foreach (var r in toRemove) TagLinks.Remove(r);

            var existing = TagLinks.Select(l => l.TagId).ToHashSet();
            foreach (var id in target.Except(existing)) TagLinks.Add(new LibraryScenarioTag(Id, id));

            Touch();
        }

        public void SetIndustries(IEnumerable<Guid>? industryIds)
        {
            var target = new HashSet<Guid>((industryIds ?? Enumerable.Empty<Guid>()).Where(x => x != Guid.Empty));

            var toRemove = IndustryLinks.Where(l => !target.Contains(l.IndustryId)).ToList();
            foreach (var r in toRemove) IndustryLinks.Remove(r);

            var existing = IndustryLinks.Select(l => l.IndustryId).ToHashSet();
            foreach (var id in target.Except(existing)) IndustryLinks.Add(new LibraryScenarioIndustry(Id, id));

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
