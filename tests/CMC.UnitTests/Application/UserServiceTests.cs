using FluentAssertions;
using CMC.Application.Ports;
using CMC.Application.Services;
using CMC.Contracts.Users;
using CMC.Domain.Common;
using CMC.Domain.Entities;
using Moq;
using Xunit;

namespace CMC.UnitTests.Application;

public class UserServiceTests {
  private readonly Mock<IUserRepository> _userRepositoryMock;
  private readonly Mock<IEmailService> _emailServiceMock;
  private readonly UserService _userService;

  public UserServiceTests() {
    _userRepositoryMock = new Mock<IUserRepository>();
    _emailServiceMock = new Mock<IEmailService>();
    _userService = new UserService(_userRepositoryMock.Object, _emailServiceMock.Object);
  }[Fact]
  public async Task RegisterAsync_WithValidData_ShouldCreateUser() {
    // Arrange
    var request = new RegisterUserRequest {
      Email = "test@example.com",
      Password = "password123",
      FirstName = "John",
      LastName = "Doe"
    };

    _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email, default)).ReturnsAsync((
      User
      ?)null);

    // Act
    var result = await _userService.RegisterAsync(request);

    // Assert
    result.Email.Should().Be(request.Email);
    result.FirstName.Should().Be(request.FirstName);
    result.LastName.Should().Be(request.LastName);

    _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), default), Times.Once);
    _emailServiceMock.Verify(x => x.SendWelcomeEmailAsync(request.Email, request.FirstName, default), Times.Once);
  }[Fact]
  public async Task RegisterAsync_WithExistingEmail_ShouldThrowException() {
    // Arrange
    var request = new RegisterUserRequest {
      Email = "test@example.com",
      Password = "password123",
      FirstName = "John",
      LastName = "Doe"
    };
    var existingUser = new User(request.Email, "hash", "Jane", "Smith");
    _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email, default)).ReturnsAsync(existingUser);

    // Act & Assert
    await _userService.Invoking(x => x.RegisterAsync(request)).Should().ThrowAsync<DomainException>().WithMessage("User with this email already exists");
  }[Fact]
  public async Task LoginAsync_WithValidCredentials_ShouldReturnUser() {
    // Arrange
    var password = "password123";
    var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
    var user = new User("test@example.com", passwordHash, "John", "Doe");
    var request = new LoginRequest {
      Email = "test@example.com",
      Password = "password123"
    };

    _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email, default)).ReturnsAsync(user);

    // Act
    var result = await _userService.LoginAsync(request);

    // Assert
    result.Should().NotBeNull();
    result !.Email.Should().Be(user.Email);
    _userRepositoryMock.Verify(x => x.UpdateAsync(user, default), Times.Once);
  }[Fact]
  public async Task LoginAsync_WithInvalidCredentials_ShouldReturnNull() {
    // Arrange
    var passwordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
    var user = new User("test@example.com", passwordHash, "John", "Doe");
    var request = new LoginRequest {
      Email = "test@example.com",
      Password = "password123"
    };

    _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email, default)).ReturnsAsync(user);

    // Act
    var result = await _userService.LoginAsync(request);

    // Assert
    result.Should().BeNull();
  }
}
