using System;
using CMC.Domain.ValueObjects;

namespace CMC.Domain.Entities;

/// <summary>
/// Domain entity representing a user in the CMC system.
/// Implements domain-driven design principles with encapsulated state and behavior.
/// Handles user authentication, email verification, password reset functionality, and customer association.
/// </summary>
public class User
{
	#region Properties - Identity & Authentication

	public Guid Id { get; private set; }
	public Email Email { get; private set; } = new Email("placeholder@example.invalid");
	public string PasswordHash { get; private set; } = string.Empty;

	#endregion

	#region Properties - Personal Information

	public string FirstName { get; private set; } = string.Empty;
	public string LastName { get; private set; } = string.Empty;
	public string Role { get; private set; } = string.Empty;
	public string Department { get; private set; } = string.Empty;

	#endregion

	#region Properties - Customer Relationship

	public Guid? CustomerId { get; private set; }
	public virtual Customer? Customer { get; private set; }

	#endregion

	#region Properties - Account Status & Tracking

	public bool IsEmailConfirmed { get; private set; }
	public DateTimeOffset CreatedAt { get; private set; }
	public DateTimeOffset? LastLoginAt { get; private set; }

	#endregion

	#region Properties - Password Reset

	public string? PasswordResetToken { get; private set; }
	public DateTimeOffset? PasswordResetTokenExpiry { get; private set; }

	#endregion

	#region Constructors

	private User() { }

	public User(Email email, string passwordHash, string firstName, string lastName, string role = "", string department = "")
	{
		if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash cannot be null or empty", nameof(passwordHash));
		if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name cannot be null or empty", nameof(firstName));
		if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));

		Id = Guid.NewGuid();
		Email = email ?? throw new ArgumentNullException(nameof(email));
		PasswordHash = passwordHash;
		FirstName = firstName.Trim();
		LastName = lastName.Trim();
		Role = (role ?? string.Empty).Trim();
		Department = (department ?? string.Empty).Trim();
		IsEmailConfirmed = false;
		CreatedAt = DateTimeOffset.UtcNow;
	}

	#endregion

	#region Domain Methods - Email

	public void ConfirmEmail()
	{
		IsEmailConfirmed = true;
	}

	public void ChangeEmail(Email newEmail)
	{
		Email = newEmail ?? throw new ArgumentNullException(nameof(newEmail));
		// Optional: IsEmailConfirmed = false;
	}

	#endregion

	#region Domain Methods - Authentication Tracking

	public void UpdateLastLogin()
	{
		LastLoginAt = DateTimeOffset.UtcNow;
	}

	#endregion

	#region Domain Methods - Personal Information

	public void UpdatePersonalInfo(string firstName, string lastName, string? role = null, string? department = null)
	{
		if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name cannot be null or empty", nameof(firstName));
		if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));

		FirstName = firstName.Trim();
		LastName = lastName.Trim();

		if (role != null) Role = role.Trim();
		if (department != null) Department = department.Trim();
	}

	#endregion

	#region Domain Methods - Customer Association

	public void AssignToCustomer(Customer customer)
	{
		if (customer == null) throw new ArgumentNullException(nameof(customer));

		if (Customer != null)
		{
			Customer.RemoveUser(this);
		}

		CustomerId = customer.Id;
		Customer = customer;
		customer.AddUser(this);
	}

	public void RemoveFromCustomer()
	{
		if (Customer != null)
		{
			Customer.RemoveUser(this);
			Customer = null;
		}
		CustomerId = null;
	}

	#endregion

	#region Domain Methods - Password Reset

	public void SetPasswordResetToken(string token, DateTimeOffset expiryUtc)
	{
		if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("Token cannot be null or empty", nameof(token));
		if (expiryUtc <= DateTimeOffset.UtcNow) throw new ArgumentException("Expiry must be in the future", nameof(expiryUtc));

		PasswordResetToken = token;
		PasswordResetTokenExpiry = expiryUtc.ToUniversalTime();
	}

	public void ClearPasswordResetToken()
	{
		PasswordResetToken = null;
		PasswordResetTokenExpiry = null;
	}

	public void UpdatePassword(string newPasswordHash)
	{
		if (string.IsNullOrWhiteSpace(newPasswordHash)) throw new ArgumentException("Password hash cannot be null or empty", nameof(newPasswordHash));

		PasswordHash = newPasswordHash;
		ClearPasswordResetToken();
	}

	#endregion
}
