using System;
using CMC.Domain.ValueObjects;

namespace CMC.Domain.Entities;

/// <summary>
/// Domain entity representing a user in the CMC system.
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

    #region Properties - Two-Factor Authentication
    public string? TwoFASecret { get; private set; }
    public bool TwoFAEnabled => !string.IsNullOrWhiteSpace(TwoFASecret);
    public DateTimeOffset? TwoFAEnabledAt { get; private set; }
    public string? TwoFABackupCodes { get; private set; }
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
    public void ConfirmEmail() => IsEmailConfirmed = true;

    public void ChangeEmail(Email newEmail)
    {
        Email = newEmail ?? throw new ArgumentNullException(nameof(newEmail));
    }
    #endregion

    #region Domain Methods - Authentication Tracking
    public void UpdateLastLogin() => LastLoginAt = DateTimeOffset.UtcNow;
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
        if (Customer != null) Customer.RemoveUser(this);

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

    /// <summary>
    /// Changes the password (hash) and invalidates any active reset token.
    /// </summary>
    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be null or empty", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        ClearPasswordResetToken();
    }

    /// <summary>
    /// Backwards-compatibility for existing tests/callers expecting UpdatePassword.
    /// Delegates to <see cref="ChangePassword"/>.
    /// </summary>
    public void UpdatePassword(string newPasswordHash) => ChangePassword(newPasswordHash);
    #endregion

    #region Domain Methods - Two-Factor Authentication
    /// <summary>
    /// Enables 2FA for the user with the provided secret and backup codes.
    /// </summary>
    public void EnableTwoFA(string secret, string? backupCodes = null)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("Secret cannot be null or empty", nameof(secret));

        TwoFASecret = secret;
        TwoFAEnabledAt = DateTimeOffset.UtcNow;
        TwoFABackupCodes = backupCodes;
    }

    /// <summary>
    /// Disables 2FA for the user and clears all related data.
    /// </summary>
    public void DisableTwoFA()
    {
        TwoFASecret = null;
        TwoFAEnabledAt = null;
        TwoFABackupCodes = null;
    }

    /// <summary>
    /// Updates the 2FA secret (for re-setup scenarios).
    /// </summary>
    public void UpdateTwoFASecret(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("Secret cannot be null or empty", nameof(secret));

        TwoFASecret = secret;
        if (!TwoFAEnabledAt.HasValue)
            TwoFAEnabledAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates the backup codes for 2FA.
    /// </summary>
    public void UpdateTwoFABackupCodes(string? backupCodes)
    {
        TwoFABackupCodes = backupCodes;
    }
    #endregion
}
