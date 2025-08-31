using System;
using System.Collections.Generic;
using System.Linq;
using CMC.Domain.Entities.Joins;

namespace CMC.Domain.Entities
{
    public class Customer
    {
        #region Properties - Identity
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        #endregion

        #region Properties - Business Information
        public int EmployeeCount { get; private set; }
        public decimal RevenuePerYear { get; private set; }
        #endregion

        #region Properties - Account Status & Tracking
        public bool IsActive { get; private set; } = true;
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }
        #endregion

        #region Navigation (Join-only + 1:n)
        // Nur explizite Join-Entities (keine Skip-Navigation "Industries")
        public virtual ICollection<CustomerIndustry> CustomerIndustries { get; private set; } = new List<CustomerIndustry>();

        // 1:n Customer -> Controls / Users
        public virtual ICollection<Control> Controls { get; private set; } = new List<Control>();
        public virtual ICollection<User> Users { get; private set; } = new List<User>();
        #endregion

        #region Constructors
        private Customer() { }

        public Customer(string name, int employeeCount, decimal revenuePerYear)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Customer name cannot be null or empty", nameof(name));
            if (employeeCount < 0) throw new ArgumentException("Employee count cannot be negative", nameof(employeeCount));
            if (revenuePerYear < 0) throw new ArgumentException("Revenue cannot be negative", nameof(revenuePerYear));

            Id = Guid.NewGuid();
            Name = name.Trim();
            EmployeeCount = employeeCount;
            RevenuePerYear = revenuePerYear;

            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
            IsActive = true;
        }
        #endregion

        #region Domain Methods - Business Information Updates
        public void UpdateBusinessInfo(string name, int employeeCount, decimal revenuePerYear)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Customer name cannot be null or empty", nameof(name));
            if (employeeCount < 0) throw new ArgumentException("Employee count cannot be negative", nameof(employeeCount));
            if (revenuePerYear < 0) throw new ArgumentException("Revenue cannot be negative", nameof(revenuePerYear));

            Name = name.Trim();
            EmployeeCount = employeeCount;
            RevenuePerYear = revenuePerYear;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        #endregion

        #region Domain Methods - Account Status
        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        #endregion

        #region Domain Methods - Industries (Join-only API)
        /// <summary>
        /// Ersetzt die Zuordnung zu Branchen (Industry) anhand einer Menge von Industry-Ids.
        /// </summary>
        public void SetIndustries(IEnumerable<Guid>? industryIds)
        {
            var target = (industryIds ?? Enumerable.Empty<Guid>())
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToHashSet();

            // Remove missing links
            var toRemove = CustomerIndustries.Where(l => !target.Contains(l.IndustryId)).ToList();
            foreach (var r in toRemove) CustomerIndustries.Remove(r);

            // Add new links
            var existing = CustomerIndustries.Select(l => l.IndustryId).ToHashSet();
            foreach (var id in target)
            {
                if (!existing.Contains(id))
                    CustomerIndustries.Add(new CustomerIndustry(customerId: Id, industryId: id));
            }

            UpdatedAt = DateTimeOffset.UtcNow;
        }
public void AddIndustries(IEnumerable<Guid> industryIds)
{
    var current = GetIndustryIds();
    var combined = current.Concat(industryIds).Distinct().ToArray();
    SetIndustries(combined);
}

public void RemoveIndustries(IEnumerable<Guid> industryIds)
{
    var current = GetIndustryIds();
    var remaining = current.Except(industryIds).ToArray();
    SetIndustries(remaining);
}
        public IReadOnlyList<Guid> GetIndustryIds()
            => CustomerIndustries.Select(l => l.IndustryId).Distinct().ToArray();
        #endregion

        #region Domain Methods - Users (1:n Helfer)
        public void AddUser(User user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (!Users.Any(u => u.Id == user.Id)) Users.Add(user);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void RemoveUser(User user)
        {
            if (user is null) return;
            var existing = Users.FirstOrDefault(u => u.Id == user.Id);
            if (existing != null) Users.Remove(existing);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        #endregion
    }
}
