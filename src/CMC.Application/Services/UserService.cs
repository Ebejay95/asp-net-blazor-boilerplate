using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using CMC.Application.Ports;
using CMC.Contracts.Users;
using CMC.Application.Ports.Mail;
using CMC.Domain.Common;
using CMC.Domain.Entities;
using CMC.Domain.ValueObjects;

namespace CMC.Application.Services;

/// <summary>
/// Application service for managing user-related business operations.
/// Handles user registration, authentication, password management, user data retrieval, customer associations, and 2FA.
/// </summary>
public class UserService
{
    #region Fields

    private readonly IUserRepository _userRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IEmailService _emailService;

    #endregion

    #region Constructor

    public UserService(IUserRepository userRepository, ICustomerRepository customerRepository, IEmailService emailService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
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

    public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
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
            "Sie haben das Zurücksetzen Ihres Passworts angefragt. Mit diesem Link könenn Sie dies vornehmen:",
            new[]
            {
                 new EmailButton("Zurücksetzen", $"/reset-password?token={token}")
            }
        );
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var user = await _userRepository.GetByPasswordResetTokenAsync(request.Token, cancellationToken);
        if (user == null || !user.PasswordResetTokenExpiry.HasValue || user.PasswordResetTokenExpiry.Value < DateTimeOffset.UtcNow)
            return false;

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.ChangePassword(passwordHash);
        await _userRepository.UpdateAsync(user, cancellationToken);

        return true;
    }

    #endregion

    #region Two-Factor Authentication Operations

    /// <summary>
    /// Generates a new TOTP secret for 2FA setup.
    /// </summary>
    public string GenerateTwoFASecret()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var random = new Random();
        var result = new StringBuilder();

        for (int i = 0; i < 32; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }

        return result.ToString();
    }

    /// <summary>
    /// Verifies a TOTP code against a secret.
    /// </summary>
    public async Task<bool> VerifyTOTPCodeAsync(string secret, string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code))
            return false;

        try
        {
            var secretBytes = Base32Decode(secret);
            var currentTimeStep = GetCurrentTimeStep();

            // Prüfe aktuellen und vorherigen/nächsten Zeitschritt (für Zeittoleranz)
            for (int i = -1; i <= 1; i++)
            {
                var timeStep = currentTimeStep + i;
                var expectedCode = GenerateTOTPCode(secretBytes, timeStep);

                if (expectedCode == code)
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    /// <summary>
    /// Enables 2FA for a user with the provided secret.
    /// </summary>
    public async Task<bool> EnableTwoFAAsync(Guid userId, string secret, string confirmationCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            // Verifiziere den Bestätigungscode
            var isValidCode = await VerifyTOTPCodeAsync(secret, confirmationCode, cancellationToken);
            if (!isValidCode)
                return false;

            // Generiere Backup-Codes
            var backupCodes = GenerateBackupCodes();

            user.EnableTwoFA(secret, string.Join(",", backupCodes));
            await _userRepository.UpdateAsync(user, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    /// <summary>
    /// Updates the 2FA secret for a user (used during setup process).
    /// </summary>
    public async Task UpdateTwoFASecretAsync(Guid userId, string? secret, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                throw new InvalidOperationException("User not found");

            if (string.IsNullOrWhiteSpace(secret))
            {
                user.DisableTwoFA();
            }
            else
            {
                user.UpdateTwoFASecret(secret);
            }

            await _userRepository.UpdateAsync(user, cancellationToken);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// Disables 2FA for a user.
    /// </summary>
    public async Task<bool> DisableTwoFAAsync(Guid userId, string currentCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            if (string.IsNullOrWhiteSpace(user.TwoFASecret))
                return true; // Already disabled

            // Verifiziere den aktuellen Code
            var isValidCode = await VerifyTOTPCodeAsync(user.TwoFASecret, currentCode, cancellationToken);
            if (!isValidCode)
                return false;

            user.DisableTwoFA();
            await _userRepository.UpdateAsync(user, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    /// <summary>
    /// Generates QR code data URL for 2FA setup.
    /// </summary>
    public string GenerateQRCodeDataUrl(string secret, string email, string issuer = "CMC App")
    {
        var otpAuthUrl = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&algorithm=SHA1&digits=6&period=30";

        // Simple QR Code generation using ASCII art approach or return URL for manual entry
        return otpAuthUrl;
    }

    /// <summary>
    /// Generates backup codes for 2FA recovery.
    /// </summary>
    private static string[] GenerateBackupCodes(int count = 10)
    {
        var codes = new string[count];
        var random = new Random();

        for (int i = 0; i < count; i++)
        {
            codes[i] = random.Next(100000, 999999).ToString();
        }

        return codes;
    }

    #endregion

    #region Private TOTP Helper Methods

    private static long GetCurrentTimeStep()
    {
        var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return unixTime / 30; // 30-Sekunden-Fenster
    }

    private static string GenerateTOTPCode(byte[] secretBytes, long timeStep)
    {
        var timeBytes = BitConverter.GetBytes(timeStep);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(timeBytes);

        using var hmac = new HMACSHA1(secretBytes);
        var hash = hmac.ComputeHash(timeBytes);

        var offset = hash[^1] & 0x0F;
        var truncatedHash = ((hash[offset] & 0x7F) << 24) |
                           ((hash[offset + 1] & 0xFF) << 16) |
                           ((hash[offset + 2] & 0xFF) << 8) |
                           (hash[offset + 3] & 0xFF);

        var code = truncatedHash % 1000000;
        return code.ToString("D6");
    }

    private static byte[] Base32Decode(string base32)
    {
        if (string.IsNullOrEmpty(base32))
            throw new ArgumentException("Base32 string cannot be null or empty");

        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var result = new List<byte>();
        var buffer = 0;
        var bufferSize = 0;

        foreach (var c in base32.ToUpperInvariant())
        {
            if (c == '=') break; // Padding

            var value = alphabet.IndexOf(c);
            if (value < 0)
                throw new ArgumentException($"Invalid Base32 character: {c}");

            buffer = (buffer << 5) | value;
            bufferSize += 5;

            if (bufferSize >= 8)
            {
                result.Add((byte)(buffer >> (bufferSize - 8)));
                bufferSize -= 8;
            }
        }

        return result.ToArray();
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
        var dto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Department = user.Department,
            IsEmailConfirmed = user.IsEmailConfirmed,
            CustomerId = user.CustomerId,
            CustomerName = user.Customer?.Name,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            TwoFASecret = user.TwoFASecret,
            TwoFAEnabled = user.TwoFAEnabled,
            TwoFAEnabledAt = user.TwoFAEnabledAt
        };
        return Task.FromResult(dto);
    }

    #endregion
}
