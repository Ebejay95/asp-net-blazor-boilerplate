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

    /// <summary>
    /// Unique identifier for the user. Generated automatically upon creation.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// User's email address as a validated value object.
    /// Serves as unique identifier for authentication.
    /// </summary>
    public Email Email { get; private set; } = new Email("placeholder@example.invalid");

    /// <summary>
    /// BCrypt hashed password for secure authentication.
    /// Never stores plain text passwords for security compliance.
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    #endregion

    #region Properties - Personal Information

    /// <summary>
    /// User's first name for personalization and display purposes.
    /// </summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>
    /// User's last name for personalization and display purposes.
    /// </summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>
    /// User's role within their organization (e.g., "Manager", "Developer", "Admin").
    /// Not scaffolded in UI forms.
    /// </summary>
    public string Role { get; private set; } = string.Empty;

    /// <summary>
    /// Department the user belongs to (e.g., "IT", "Sales", "Marketing").
    /// Not scaffolded in UI forms.
    /// </summary>
    public string Department { get; private set; } = string.Empty;

    #endregion

    #region Properties - Customer Relationship

    /// <summary>
    /// Foreign key to the customer this user belongs to.
    /// Null if user is not associated with any customer.
    /// </summary>
    public Guid? CustomerId { get; private set; }

    /// <summary>
    /// Navigation property to the customer this user belongs to.
    /// Represents the company/organization the user works for.
    /// </summary>
    public virtual Customer? Customer { get; private set; }

    #endregion

    #region Properties - Account Status & Tracking

    /// <summary>
    /// Indicates whether the user has confirmed their email address.
    /// Used to enforce email verification before certain operations.
    /// </summary>
    public bool IsEmailConfirmed { get; private set; }

    /// <summary>
    /// UTC timestamp when the user account was created.
    /// Immutable after creation for audit purposes.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// UTC timestamp of the user's most recent successful login.
    /// Null if the user has never logged in.
    /// </summary>
    public DateTime? LastLoginAt { get; private set; }

    #endregion

    #region Properties - Password Reset

    /// <summary>
    /// Secure token for password reset operations.
    /// Null when no password reset is in progress.
    /// </summary>
    public string? PasswordResetToken { get; private set; }

    /// <summary>
    /// UTC expiry time for the password reset token.
    /// Null when no password reset is in progress.
    /// </summary>
    public DateTime? PasswordResetTokenExpiry { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Private parameterless constructor for Entity Framework.
    /// Prevents direct instantiation without required parameters.
    /// </summary>
    private User() { }

    /// <summary>
    /// Creates a new user with the specified details.
    /// Initializes the user in an unconfirmed state requiring email verification.
    /// </summary>
    /// <param name="email">User's email address (validated value object; implicit cast from string möglich)</param>
    /// <param name="passwordHash">BCrypt hashed password</param>
    /// <param name="firstName">User's first name</param>
    /// <param name="lastName">User's last name</param>
    /// <param name="role">User's role (optional)</param>
    /// <param name="department">User's department (optional)</param>
    /// <exception cref="ArgumentException">Thrown when any required parameter is null or empty</exception>
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
        CreatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Domain Methods - Email

    /// <summary>
    /// Confirms the user's email address after successful verification.
    /// Enables full account functionality that requires verified email.
    /// </summary>
    public void ConfirmEmail()
    {
        IsEmailConfirmed = true;
    }

    /// <summary>
    /// Changes the user's email address (validated by value object).
    /// </summary>
    /// <param name="newEmail">New email (value object; implicit cast from string möglich)</param>
    public void ChangeEmail(Email newEmail)
    {
        Email = newEmail ?? throw new ArgumentNullException(nameof(newEmail));
        // Optional: Beim E-Mail-Wechsel könnte man IsEmailConfirmed zurücksetzen, falls gewünscht:
        // IsEmailConfirmed = false;
    }

    #endregion

    #region Domain Methods - Authentication Tracking

    /// <summary>
    /// Updates the last login timestamp to the current UTC time.
    /// Called after successful authentication to track user activity.
    /// </summary>
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    #endregion

    #region Domain Methods - Personal Information

    /// <summary>
    /// Updates the user's personal information.
    /// </summary>
    /// <param name="firstName">Updated first name</param>
    /// <param name="lastName">Updated last name</param>
    /// <param name="role">Updated role (optional)</param>
    /// <param name="department">Updated department (optional)</param>
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

    /// <summary>
    /// Assigns the user to a customer (company).
    /// </summary>
    /// <param name="customer">The customer to assign to</param>
    public void AssignToCustomer(Customer customer)
    {
        if (customer == null) throw new ArgumentNullException(nameof(customer));

        // Remove from current customer if any
        if (Customer != null)
        {
            Customer.RemoveUser(this);
        }

        CustomerId = customer.Id;
        Customer = customer;
        customer.AddUser(this);
    }

    /// <summary>
    /// Removes the user from their current customer.
    /// </summary>
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

    /// <summary>
    /// Sets a password reset token with expiry time for secure password recovery.
    /// Overwrites any existing token to ensure only one active reset per user.
    /// </summary>
    /// <param name="token">Cryptographically secure reset token</param>
    /// <param name="expiry">UTC expiry time for the token</param>
    /// <exception cref="ArgumentException">Thrown when token is null or empty</exception>
    /// <exception cref="ArgumentException">Thrown when expiry is in the past</exception>
    public void SetPasswordResetToken(string token, DateTime expiry)
    {
        if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("Token cannot be null or empty", nameof(token));
        if (expiry <= DateTime.UtcNow) throw new ArgumentException("Expiry must be in the future", nameof(expiry));

        PasswordResetToken = token;
        PasswordResetTokenExpiry = expiry;
    }

    /// <summary>
    /// Clears the password reset token and expiry.
    /// Called after successful password reset or token expiration.
    /// </summary>
    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
    }

    /// <summary>
    /// Updates the user's password with a new hash and clears any active reset tokens.
    /// Ensures security by invalidating all existing reset tokens after password change.
    /// </summary>
    /// <param name="newPasswordHash">New BCrypt hashed password</param>
    /// <exception cref="ArgumentException">Thrown when password hash is null or empty</exception>
    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash)) throw new ArgumentException("Password hash cannot be null or empty", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        ClearPasswordResetToken();
    }

    #endregion
}
