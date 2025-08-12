using FluentAssertions;
using CMC.Domain.Entities;
using Xunit;

namespace CMC.UnitTests.Domain;

public class UserTests {
  [Fact]
  public void User_WhenCreated_ShouldSetPropertiesCorrectly() {
    // Arrange
    var email = "test@example.com";
    var passwordHash = "hashedpassword";
    var firstName = "John";
    var lastName = "Doe";

    // Act
    var user = new User(email, passwordHash, firstName, lastName);

    // Assert
    user.Id.Should().NotBeEmpty();
    user.Email.Should().Be(email);
    user.PasswordHash.Should().Be(passwordHash);
    user.FirstName.Should().Be(firstName);
    user.LastName.Should().Be(lastName);
    user.IsEmailConfirmed.Should().BeFalse();
    user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    user.LastLoginAt.Should().BeNull();
  }[Fact]
  public void ConfirmEmail_ShouldSetIsEmailConfirmedToTrue() {
    // Arrange
    var user = new User("test@example.com", "hash", "John", "Doe");

    // Act
    user.ConfirmEmail();

    // Assert
    user.IsEmailConfirmed.Should().BeTrue();
  }[Fact]
  public void UpdateLastLogin_ShouldSetLastLoginAt() {
    // Arrange
    var user = new User("test@example.com", "hash", "John", "Doe");

    // Act
    user.UpdateLastLogin();

    // Assert
    user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
  }[Fact]
  public void SetPasswordResetToken_ShouldSetTokenAndExpiry() {
    // Arrange
    var user = new User("test@example.com", "hash", "John", "Doe");
    var token = "reset-token";
    var expiry = DateTime.UtcNow.AddHours(1);

    // Act
    user.SetPasswordResetToken(token, expiry);

    // Assert
    user.PasswordResetToken.Should().Be(token);
    user.PasswordResetTokenExpiry.Should().Be(expiry);
  }[Fact]
  public void UpdatePassword_ShouldUpdatePasswordAndClearResetToken() {
    // Arrange
    var user = new User("test@example.com", "oldHash", "John", "Doe");
    user.SetPasswordResetToken("token", DateTime.UtcNow.AddHours(1));
    var newPasswordHash = "newHash";

    // Act
    user.UpdatePassword(newPasswordHash);

    // Assert
    user.PasswordHash.Should().Be(newPasswordHash);
    user.PasswordResetToken.Should().BeNull();
    user.PasswordResetTokenExpiry.Should().BeNull();
  }
}
