using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using BCrypt.Net;
using CMC.Application.Ports;
using CMC.Contracts.Users;
using CMC.Domain.Common;
using CMC.Domain.Entities;

namespace CMC.Application.Services;

public class UserService {
  private readonly IUserRepository _userRepository;
  private readonly IEmailService _emailService;

  public UserService(IUserRepository userRepository, IEmailService emailService) {
    _userRepository = userRepository;
    _emailService = emailService;
  }

  public async Task<UserDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default) {
    var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
    if (existingUser != null)
      throw new DomainException("User with this email already exists");

    var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
    var user = new User(request.Email, passwordHash, request.FirstName, request.LastName);

    await _userRepository.AddAsync(user, cancellationToken);
    await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName, cancellationToken);

    return MapToDto(user);
  }

  public async Task<UserDto?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default) {
    var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
    if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
      return null;

    user.UpdateLastLogin();
    await _userRepository.UpdateAsync(user, cancellationToken);

    return MapToDto(user);
  }

  public async Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default) {
    var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
    if (user == null)
      return; // Don't reveal if email exists

    var token = GenerateSecureToken();
    var expiry = DateTime.UtcNow.AddHours(1);

    user.SetPasswordResetToken(token, expiry);
    await _userRepository.UpdateAsync(user, cancellationToken);
    await _emailService.SendPasswordResetEmailAsync(email, token, cancellationToken);
  }

  public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default) {
    var user = await _userRepository.GetByPasswordResetTokenAsync(request.Token, cancellationToken);
    if (user == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
      return false;

    var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
    user.UpdatePassword(passwordHash);
    await _userRepository.UpdateAsync(user, cancellationToken);

    return true;
  }

  private static string GenerateSecureToken() {
    var bytes = new byte[32];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(bytes);
    return Convert.ToBase64String(bytes).Replace("/", "_").Replace("+", "-").TrimEnd('=');
  }

  private static UserDto MapToDto(User user) => new(user.Id, user.Email, user.FirstName, user.LastName, user.IsEmailConfirmed, user.CreatedAt, user.LastLoginAt);
}
