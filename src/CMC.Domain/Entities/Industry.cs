using System;
using System.Collections.Generic;
using CMC.Domain.Entities.Joins;

namespace CMC.Domain.Entities
{
    public class Industry : ISoftDeletable, IVersionedEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;

        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        // Joins
        public virtual ICollection<CustomerIndustry> CustomerIndustries { get; private set; } = new List<CustomerIndustry>();
        public virtual ICollection<FrameworkIndustry> FrameworkIndustries { get; private set; } = new List<FrameworkIndustry>();
        public virtual ICollection<LibraryControlIndustry> LibraryControlIndustries { get; private set; } = new List<LibraryControlIndustry>();
        public virtual ICollection<LibraryScenarioIndustry> LibraryScenarioIndustries { get; private set; } = new List<LibraryScenarioIndustry>();
        public virtual ICollection<ControlIndustry> ControlIndustries { get; private set; } = new List<ControlIndustry>();

        private Industry() { }
        public Industry(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
            Id = Guid.NewGuid();
            Name = name.Trim();
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
            Name = name.Trim();
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
            }
        }
    }
}
