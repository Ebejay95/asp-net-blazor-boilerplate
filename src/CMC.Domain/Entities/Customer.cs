using System;

namespace CMC.Domain.Entities;

/// <summary>
/// Domain entity representing a Customer (Company) in the CMC system.
/// Implements domain-driven design principles with encapsulated state and behavior.
/// Represents business customers that can have multiple users associated with them.
/// </summary>
public class Customer
{
    #region Properties - Identity

    /// <summary>
    /// Unique identifier for the Customer. Generated automatically upon creation.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Company/Customer name for identification and display purposes.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    #endregion

    #region Properties - Business Information

    /// <summary>
    /// Industry sector the customer operates in (e.g., "Technology", "Healthcare", "Manufacturing").
    /// </summary>
    public string Industry { get; private set; } = string.Empty;

    /// <summary>
    /// Number of employees working at the customer company.
    /// Used for customer segmentation and service planning.
    /// </summary>
    public int EmployeeCount { get; private set; }

    /// <summary>
    /// Annual revenue of the customer company in the base currency.
    /// Used for customer segmentation and pricing strategies.
    /// </summary>
    public decimal RevenuePerYear { get; private set; }

    #endregion

    #region Properties - Account Status & Tracking

    /// <summary>
    /// Indicates whether the customer account is active and in good standing.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// UTC timestamp when the customer account was created.
    /// Immutable after creation for audit purposes.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// UTC timestamp when the customer information was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Collection of users associated with this customer.
    /// Represents employees or contacts working for this customer company.
    /// </summary>
    public virtual ICollection<User> Users { get; private set; } = new List<User>();

    #endregion

    #region Constructors

    /// <summary>
    /// Private parameterless constructor for Entity Framework.
    /// Prevents direct instantiation without required parameters.
    /// </summary>
    private Customer() { }

    /// <summary>
    /// Creates a new customer with the specified business details.
    /// </summary>
    /// <param name="name">Company name</param>
    /// <param name="industry">Industry sector</param>
    /// <param name="employeeCount">Number of employees</param>
    /// <param name="revenuePerYear">Annual revenue</param>
    /// <exception cref="ArgumentException">Thrown when any parameter is invalid</exception>
    public Customer(string name, string industry, int employeeCount, decimal revenuePerYear)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Customer name cannot be null or empty", nameof(name));
        if (string.IsNullOrWhiteSpace(industry))
            throw new ArgumentException("Industry cannot be null or empty", nameof(industry));
        if (employeeCount < 0)
            throw new ArgumentException("Employee count cannot be negative", nameof(employeeCount));
        if (revenuePerYear < 0)
            throw new ArgumentException("Revenue cannot be negative", nameof(revenuePerYear));

        Id = Guid.NewGuid();
        Name = name.Trim();
        Industry = industry.Trim();
        EmployeeCount = employeeCount;
        RevenuePerYear = revenuePerYear;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    #endregion

    #region Domain Methods - Business Information Updates

    /// <summary>
    /// Updates the customer's business information.
    /// </summary>
    /// <param name="name">Updated company name</param>
    /// <param name="industry">Updated industry</param>
    /// <param name="employeeCount">Updated employee count</param>
    /// <param name="revenuePerYear">Updated annual revenue</param>
    public void UpdateBusinessInfo(string name, string industry, int employeeCount, decimal revenuePerYear)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Customer name cannot be null or empty", nameof(name));
        if (string.IsNullOrWhiteSpace(industry))
            throw new ArgumentException("Industry cannot be null or empty", nameof(industry));
        if (employeeCount < 0)
            throw new ArgumentException("Employee count cannot be negative", nameof(employeeCount));
        if (revenuePerYear < 0)
            throw new ArgumentException("Revenue cannot be negative", nameof(revenuePerYear));

        Name = name.Trim();
        Industry = industry.Trim();
        EmployeeCount = employeeCount;
        RevenuePerYear = revenuePerYear;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Domain Methods - Account Status

    /// <summary>
    /// Deactivates the customer account.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivates the customer account.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Domain Methods - User Association

    /// <summary>
    /// Associates a user with this customer.
    /// Simple internal method that just manages the collection.
    /// </summary>
    /// <param name="user">The user to associate</param>
    internal void AddUser(User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        if (!Users.Contains(user))
        {
            Users.Add(user);
        }
    }

    /// <summary>
    /// Removes a user association from this customer.
    /// Simple internal method that just manages the collection.
    /// </summary>
    /// <param name="user">The user to remove</param>
    internal void RemoveUser(User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        if (Users.Contains(user))
        {
            Users.Remove(user);
        }
    }

    #endregion
}
