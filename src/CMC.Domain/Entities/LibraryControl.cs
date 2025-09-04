using System;
using System.Collections.Generic;
using System.Linq;
using CMC.Domain.Entities.Joins;

namespace CMC.Domain.Entities
{
    public class LibraryControl
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;

        public decimal CapexEur { get; private set; }
        public decimal OpexYearEur { get; private set; }

        public int InternalDays { get; private set; }
        public int ExternalDays { get; private set; }
        public int TotalDays { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

        // --- Soft Delete Felder: von deiner EF-Konfiguration erwartet ---
        public bool IsDeleted { get; set; }                // public set: EF/Repo können setzen
        public string? DeletedBy { get; set; }             // public set
        public DateTimeOffset? DeletedAt { get; set; }     // public set

        // --- M:N-Links ---
        public List<LibraryControlTag> TagLinks { get; private set; } = new();
        public List<LibraryControlIndustry> IndustryLinks { get; private set; } = new();
        public List<LibraryControlScenario> ScenarioLinks { get; private set; } = new();

        // Von deiner Config verlangt:
        public List<LibraryControlFramework> FrameworkLinks { get; private set; } = new();

        protected LibraryControl() { }

        public LibraryControl(
            string name,
            decimal capexEur,
            decimal opexYearEur,
            int internalDays,
            int externalDays,
            IEnumerable<Guid>? tagIds = null,
            IEnumerable<Guid>? industryIds = null,
            int? totalDays = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name required", nameof(name));

            Id = Guid.NewGuid();
            Name = name.Trim();

            CapexEur = capexEur;
            OpexYearEur = opexYearEur;

            InternalDays = internalDays;
            ExternalDays = externalDays;
            TotalDays = totalDays ?? (internalDays + externalDays);

            SetTags(tagIds);
            SetIndustries(industryIds);
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name required", nameof(name));

            var trimmed = name.Trim();
            if (!string.Equals(trimmed, Name, StringComparison.Ordinal))
            {
                Name = trimmed;
                Touch();
            }
        }

        public void UpdateCosts(decimal capexEur, decimal opexYearEur)
        {
            var changed = false;
            if (CapexEur != capexEur) { CapexEur = capexEur; changed = true; }
            if (OpexYearEur != opexYearEur) { OpexYearEur = opexYearEur; changed = true; }
            if (changed) Touch();
        }

        public void UpdateEffort(int internalDays, int externalDays, int? totalDays)
        {
            var changed = false;

            if (InternalDays != internalDays) { InternalDays = internalDays; changed = true; }
            if (ExternalDays != externalDays) { ExternalDays = externalDays; changed = true; }

            var newTotal = totalDays ?? (internalDays + externalDays);
            if (TotalDays != newTotal) { TotalDays = newTotal; changed = true; }

            if (changed) Touch();
        }

        // ---- M:N Setter ----

        public void SetTags(IEnumerable<Guid>? ids)
        {
            var newIds = (ids ?? Array.Empty<Guid>()).Where(x => x != Guid.Empty).ToHashSet();
            var existingIds = TagLinks.Select(x => x.TagId).ToHashSet();
            var changed = false;

            if (TagLinks.RemoveAll(x => !newIds.Contains(x.TagId)) > 0) changed = true;
            foreach (var add in newIds.Except(existingIds))
            {
                TagLinks.Add(new LibraryControlTag(Id, add)); // ggf. ctor/Setter anpassen
                changed = true;
            }

            if (changed) Touch();
        }

        public void SetIndustries(IEnumerable<Guid>? ids)
        {
            var newIds = (ids ?? Array.Empty<Guid>()).Where(x => x != Guid.Empty).ToHashSet();
            var existingIds = IndustryLinks.Select(x => x.IndustryId).ToHashSet();
            var changed = false;

            if (IndustryLinks.RemoveAll(x => !newIds.Contains(x.IndustryId)) > 0) changed = true;
            foreach (var add in newIds.Except(existingIds))
            {
                IndustryLinks.Add(new LibraryControlIndustry(Id, add));
                changed = true;
            }

            if (changed) Touch();
        }

        // Library-Szenarien (Join-Property: LibraryScenarioId)
        public void SetLibraryScenarios(IEnumerable<Guid>? ids)
        {
            var newIds = (ids ?? Array.Empty<Guid>()).Where(x => x != Guid.Empty).ToHashSet();
            var existingIds = ScenarioLinks.Select(x => x.LibraryScenarioId).ToHashSet();
            var changed = false;

            if (ScenarioLinks.RemoveAll(x => !newIds.Contains(x.LibraryScenarioId)) > 0) changed = true;
            foreach (var add in newIds.Except(existingIds))
            {
                ScenarioLinks.Add(new LibraryControlScenario(Id, add));
                changed = true;
            }

            if (changed) Touch();
        }

        // (Optional) Frameworks synchronisieren – nur falls du das brauchst
        public void SetFrameworks(IEnumerable<Guid>? ids)
        {
            var newIds = (ids ?? Array.Empty<Guid>()).Where(x => x != Guid.Empty).ToHashSet();
            var existingIds = FrameworkLinks.Select(x => x.FrameworkId).ToHashSet();
            var changed = false;

            if (FrameworkLinks.RemoveAll(x => !newIds.Contains(x.FrameworkId)) > 0) changed = true;
            foreach (var add in newIds.Except(existingIds))
            {
                FrameworkLinks.Add(new LibraryControlFramework(Id, add));
                changed = true;
            }

            if (changed) Touch();
        }

        public Guid[] GetTagIds() => TagLinks.Select(x => x.TagId).ToArray();
        public Guid[] GetIndustryIds() => IndustryLinks.Select(x => x.IndustryId).ToArray();
        public Guid[] GetLibraryScenarioIds() => ScenarioLinks.Select(x => x.LibraryScenarioId).ToArray();

        // ---- Soft Delete API (Repository ruft parametrierloses Delete() auf) ----
        public void Delete()
        {
            if (!IsDeleted)
            {
                IsDeleted = true;
                DeletedAt = DateTimeOffset.UtcNow;
                Touch();
            }
        }

        public void Restore()
        {
            if (IsDeleted)
            {
                IsDeleted = false;
                DeletedBy = null;
                DeletedAt = null;
                Touch();
            }
        }

        private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
    }
}
