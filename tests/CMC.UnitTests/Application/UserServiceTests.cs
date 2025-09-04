using System;
using System.Threading.Tasks;
using BCrypt.Net;
using FluentAssertions;
using Moq;
using Xunit;

using CMC.Application.Ports;
using CMC.Application.Services;
using CMC.Contracts.Users;
using CMC.Domain.Common;
using CMC.Domain.Entities;

namespace CMC.UnitTests.Application
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _emailServiceMock = new Mock<IEmailService>();

            // EmailService wird nur injiziert, aber NICHT verifiziert.
            _userService = new UserService(
                _userRepositoryMock.Object,
                _customerRepositoryMock.Object,
                _emailServiceMock.Object
            );
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldCreateUser()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe",
                Role = "User",
                Department = "IT",
                CustomerId = null
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(request.Email, default))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.RegisterAsync(request);

            // Assert
            result.Email.Should().Be(request.Email);
            result.FirstName.Should().Be(request.FirstName);
            result.LastName.Should().Be(request.LastName);
            result.Role.Should().Be(request.Role);
            result.Department.Should().Be(request.Department);

            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), default), Times.Once);
            // Kein Verify auf _emailServiceMock – bewusst entfernt.
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ShouldThrowException()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe",
                Role = "User",
                Department = "IT"
            };

            var existingUser = new User(request.Email, "hash", request.FirstName, request.LastName, request.Role, request.Department);
            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(request.Email, default))
                .ReturnsAsync(existingUser);

            // Act & Assert
            await _userService
                .Invoking(svc => svc.RegisterAsync(request))
                .Should()
                .ThrowAsync<DomainException>()
                .WithMessage("User with this email already exists");
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnUser()
        {
            // Arrange
            var password = "password123";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User("test@example.com", passwordHash, "John", "Doe", "User", "IT");

            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "password123"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(request.Email, default))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(user.Email);
            _userRepositoryMock.Verify(x => x.UpdateAsync(user, default), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ShouldReturnNull()
        {
            // Arrange
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
            var user = new User("test@example.com", passwordHash, "John", "Doe", "User", "IT");

            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(request.Email, default))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.LoginAsync(request);

            // Assert
            result.Should().BeNull();
        }
    }
}
