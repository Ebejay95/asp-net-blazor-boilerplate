using System.Collections.Generic;
using CMC.Domain.Entities.Joins;

namespace CMC.Domain.Entities
{
    public class Industry
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;

        // Nur explizite Joins (keine Skip-Navs)
        public virtual ICollection<CustomerIndustry> CustomerIndustries { get; private set; } = new List<CustomerIndustry>();
        public virtual ICollection<FrameworkIndustry> FrameworkIndustries { get; private set; } = new List<FrameworkIndustry>();
        public virtual ICollection<LibraryControlIndustry> LibraryControlIndustries { get; private set; } = new List<LibraryControlIndustry>();
        public virtual ICollection<LibraryScenarioIndustry> LibraryScenarioIndustries { get; private set; } = new List<LibraryScenarioIndustry>();

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
    }
}
