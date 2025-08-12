using System.ComponentModel.DataAnnotations;

namespace CMC.Domain.Entities;

public class User {
  public Guid Id {
    get;
    private set;
  }
  public string Email {
    get;
    private set;
  } = string.Empty;
  public string PasswordHash {
    get;
    private set;
  } = string.Empty;
  public string FirstName {
    get;
    private set;
  } = string.Empty;
  public string LastName {
    get;
    private set;
  } = string.Empty;
  public bool IsEmailConfirmed {
    get;
    private set;
  }
  public DateTime CreatedAt {
    get;
    private set;
  }
  public DateTime? LastLoginAt {
    get;
    private set;
  }
  public string? PasswordResetToken {
    get;
    private set;
  }
  public DateTime? PasswordResetTokenExpiry {
    get;
    private set;
  }

  private User() {}

  public User(string email, string passwordHash, string firstName, string lastName) {
    Id = Guid.NewGuid();
    Email = email;
    PasswordHash = passwordHash;
    FirstName = firstName;
    LastName = lastName;
    IsEmailConfirmed = false;
    CreatedAt = DateTime.UtcNow;
  }

  public void ConfirmEmail() {
    IsEmailConfirmed = true;
  }

  public void UpdateLastLogin() {
    LastLoginAt = DateTime.UtcNow;
  }

  public void SetPasswordResetToken(string token, DateTime expiry) {
    PasswordResetToken = token;
    PasswordResetTokenExpiry = expiry;
  }

  public void ClearPasswordResetToken() {
    PasswordResetToken = null;
    PasswordResetTokenExpiry = null;
  }

  public void UpdatePassword(string newPasswordHash) {
    PasswordHash = newPasswordHash;
    ClearPasswordResetToken();
  }
}
