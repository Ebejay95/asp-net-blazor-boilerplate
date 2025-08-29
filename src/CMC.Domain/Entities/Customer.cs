using System;
using System.Collections.Generic;

namespace CMC.Domain.Entities;

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

	#region Navigation Properties

	public virtual ICollection<User> Users { get; private set; } = new List<User>();
	public virtual ICollection<CustomerIndustry> IndustryLinks { get; private set; } = new List<CustomerIndustry>();

	#endregion

	#region Constructors

	private Customer() { }

	public Customer(string name, int employeeCount, decimal revenuePerYear)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Customer name cannot be null or empty", nameof(name));
		if (employeeCount < 0)
			throw new ArgumentException("Employee count cannot be negative", nameof(employeeCount));
		if (revenuePerYear < 0)
			throw new ArgumentException("Revenue cannot be negative", nameof(revenuePerYear));

		Id = Guid.NewGuid();
		Name = name.Trim();
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
	/// <param name="employeeCount">Updated employee count</param>
	/// <param name="revenuePerYear">Updated annual revenue</param>
	public void UpdateBusinessInfo(string name, int employeeCount, decimal revenuePerYear)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Customer name cannot be null or empty", nameof(name));
		if (employeeCount < 0)
			throw new ArgumentException("Employee count cannot be negative", nameof(employeeCount));
		if (revenuePerYear < 0)
			throw new ArgumentException("Revenue cannot be negative", nameof(revenuePerYear));

		Name = name.Trim();
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
