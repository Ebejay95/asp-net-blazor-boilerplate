using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Contracts.Users;
using CMC.Application.Ports.Mail;
using CMC.Domain.Common;
using CMC.Domain.Entities;
using CMC.Domain.ValueObjects;

namespace CMC.Application.Services;

/// <summary>
/// Application service for managing user-related business operations.
/// Handles user registration, authentication, password management, user data retrieval, and customer associations.
/// </summary>
public class UserService
{
    #region Fields

    private readonly IUserRepository _userRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IEmailService _emailService;
    private readonly string? _baseUrl;

    #endregion

    #region Constructor

    public UserService(IUserRepository userRepository, ICustomerRepository customerRepository, IEmailService emailService, IConfiguration configuration)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _baseUrl = configuration["PUBLIC_BASE_URL"] ?? configuration["GraphMail:PublicBaseUrl"];
    }

    #endregion

    #region CREATE Operations

    /// <summary>
    /// Registers a new user in the system with email verification and welcome notification.
    /// </summary>
    public async Task<UserDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null) throw new DomainException("User with this email already exists");

        Customer? customer = null;
        if (request.CustomerId.HasValue)
        {
            customer = await _customerRepository.GetByIdAsync(request.CustomerId.Value, cancellationToken);
            if (customer == null) throw new DomainException("Specified customer does not exist");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User((Email)request.Email, passwordHash, request.FirstName, request.LastName, request.Role, request.Department);

        if (customer != null)
            user.AssignToCustomer(customer);

        await _userRepository.AddAsync(user, cancellationToken);

        return await MapToReadDtoAsync(user, cancellationToken);
    }

    #endregion

    #region READ Operations

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user != null ? await MapToReadDtoAsync(user, cancellationToken) : null;
    }

    public async Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var userDtos = new List<UserDto>(capacity: users.Count);
        foreach (var user in users)
            userDtos.Add(await MapToReadDtoAsync(user, cancellationToken));
        return userDtos;
    }

    public async Task<List<UserDto>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customerUsers = await _userRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        var userDtos = new List<UserDto>(capacity: customerUsers.Count);
        foreach (var user in customerUsers)
            userDtos.Add(await MapToReadDtoAsync(user, cancellationToken));
        return userDtos;
    }

    #endregion

    #region UPDATE Operations

    public async Task<UserDto?> UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null) return null;

        user.UpdatePersonalInfo(request.FirstName, request.LastName, request.Role, request.Department);

        if (request.IsEmailConfirmed == true)
            user.ConfirmEmail();

        if (request.CustomerId.HasValue)
        {
            if (user.Customer != null)
                user.RemoveFromCustomer();

            var newCustomer = await _customerRepository.GetByIdAsync(request.CustomerId.Value, cancellationToken);
            if (newCustomer == null) throw new DomainException("Specified customer does not exist");

            user.AssignToCustomer(newCustomer);
        }
        else if (request.CustomerId == null && user.Customer != null)
        {
            user.RemoveFromCustomer();
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
        return await MapToReadDtoAsync(user, cancellationToken);
    }

    #endregion

    #region DELETE Operations

    public async Task<bool> DeleteAsync(DeleteUserRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        return await DeleteAsync(request.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null) return false;

        if (user.Customer != null)
            user.RemoveFromCustomer();

        await _userRepository.DeleteAsync(user, cancellationToken);
        return true;
    }

    #endregion

    #region Customer Association Operations

    public async Task<UserDto?> AssignToCustomerAsync(AssignUserToCustomerRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null) return null;

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null) throw new DomainException("Customer not found");

        if (user.Customer != null)
            user.RemoveFromCustomer();

        user.AssignToCustomer(customer);
        await _userRepository.UpdateAsync(user, cancellationToken);

        return await MapToReadDtoAsync(user, cancellationToken);
    }

    public async Task<UserDto?> RemoveFromCustomerAsync(RemoveUserFromCustomerRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null) return null;

        if (user.Customer != null)
        {
            user.RemoveFromCustomer();
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        return await MapToReadDtoAsync(user, cancellationToken);
    }

    #endregion

    #region Authentication Operations

    public async Task<UserDto?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        user.UpdateLastLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);

        return await MapToReadDtoAsync(user, cancellationToken);
    }

    #endregion

    #region Password Reset Operations

    public async Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user == null) return;

        var token  = GenerateSecureToken();
        var expiry = DateTimeOffset.UtcNow.AddHours(1);

        user.SetPasswordResetToken(token, expiry);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _emailService.SendEmailAsync(
            user.Email,
            "Passwort zurücksetzen",
            "Sie haben das Zurücksetzen Ihres Passworts angefragt. Mit diesem Link können Sie dies vornehmen:",
            new[]
            {
                new EmailButton("Zurücksetzen", $"/reset-password?token={token}")
            },
            baseUrl: _baseUrl
        );
    }

public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
{
    if (request == null) throw new ArgumentNullException(nameof(request));

    var user = await _userRepository.GetByPasswordResetTokenAsync(request.Token, cancellationToken);
    if (user == null || !user.PasswordResetTokenExpiry.HasValue || user.PasswordResetTokenExpiry.Value < DateTimeOffset.UtcNow)
        return false;

    var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
    user.ChangePassword(passwordHash);                // ✅ Domänenmethode
    await _userRepository.UpdateAsync(user, cancellationToken);

    return true;
}

    #endregion

    #region Private Helper Methods

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("/", "_")
            .Replace("+", "-")
            .TrimEnd('=');
    }

    private static Task<UserDto> MapToReadDtoAsync(User user, CancellationToken cancellationToken = default)
    {
        // Keine zweite DB-Operation! Nur Werte vom bereits geladenen User verwenden.
        // CustomerName nur setzen, wenn die Navigation bereits vorhanden ist.
        var dto = new UserDto
        {
            Id              = user.Id,
            Email           = user.Email,
            FirstName       = user.FirstName,
            LastName        = user.LastName,
            Role            = user.Role,
            Department      = user.Department,
            IsEmailConfirmed= user.IsEmailConfirmed,
            CustomerId      = user.CustomerId,
            CustomerName    = user.Customer?.Name,  // bleibt null, falls nicht includiert
            CreatedAt       = user.CreatedAt,
            LastLoginAt     = user.LastLoginAt
        };
        return Task.FromResult(dto);
    }

    #endregion
}
