using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Domain.Entities;

/// <summary>
/// Domain entity representing a user in the CMC system.
/// Implements domain-driven design principles with encapsulated state and behavior.
/// Handles user authentication, email verification, and password reset functionality.
/// </summary>
public class User
{
    #region Properties - Identity & Authentication

    /// <summary>
    /// Unique identifier for the user. Generated automatically upon creation.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// User's email address. Serves as unique identifier for authentication.
    /// Must be unique across the system and is used for all communications.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

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
    /// <param name="email">User's email address (must be unique)</param>
    /// <param name="passwordHash">BCrypt hashed password</param>
    /// <param name="firstName">User's first name</param>
    /// <param name="lastName">User's last name</param>
    /// <exception cref="ArgumentException">Thrown when any parameter is null or empty</exception>
    public User(string email, string passwordHash, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be null or empty", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash cannot be null or empty", nameof(passwordHash));
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name cannot be null or empty", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));

        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        IsEmailConfirmed = false;
        CreatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Domain Methods - Email Verification

    /// <summary>
    /// Confirms the user's email address after successful verification.
    /// Enables full account functionality that requires verified email.
    /// </summary>
    public void ConfirmEmail()
    {
        IsEmailConfirmed = true;
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

    #region Domain Methods - Helpers

    public void UpdatePersonalInfo(string firstName, string lastName)
    {
      if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name cannot be null or empty", nameof(firstName));
      if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));

      FirstName = firstName;
      LastName = lastName;
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
