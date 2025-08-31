using System;
using System.Collections.Generic;

namespace CMC.Domain.Entities
{
    public class Tag : ISoftDeletable, IVersionedEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;

        // Soft Delete Properties
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        // Nur explizite Joins
        public virtual ICollection<LibraryControlTag> LibraryControlTags { get; private set; } = new List<LibraryControlTag>();
        public virtual ICollection<LibraryScenarioTag> LibraryScenarioTags { get; private set; } = new List<LibraryScenarioTag>();
        public virtual ICollection<ScenarioTag> ScenarioTags { get; private set; } = new List<ScenarioTag>();

        private Tag() { }

        public Tag(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
            Id = Guid.NewGuid();
            Name = name.Trim();
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
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
