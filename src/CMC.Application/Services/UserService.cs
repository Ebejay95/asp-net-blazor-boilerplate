using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using BCrypt.Net;
using CMC.Application.Ports;
using CMC.Contracts.Users;
using CMC.Domain.Common;
using CMC.Domain.Entities;
using System.Linq;

namespace CMC.Application.Services;

/// <summary>
/// Application service for managing user-related business operations.
/// Handles user registration, authentication, password management, and user data retrieval.
/// </summary>
public class UserService {
  #region Fields

  private readonly IUserRepository _userRepository;
  private readonly IEmailService _emailService;

  #endregion

  #region Constructor

  /// <summary>
  /// Initializes a new instance of the UserService class.
  /// </summary>
  /// <param name="userRepository">Repository for user data operations</param>
  /// <param name="emailService">Service for sending email notifications</param>
  /// <exception cref="ArgumentNullException">Thrown when any dependency is null</exception>
  public UserService(IUserRepository userRepository, IEmailService emailService) {
    _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
  }

  #endregion

  #region CREATE Operations

  /// <summary>
  /// Registers a new user in the system with email verification and welcome notification.
  /// Validates email uniqueness, hashes password securely, and sends welcome email.
  /// </summary>
  /// <param name="request">Registration details including email, password, and personal information</param>
  /// <param name="cancellationToken">Token to cancel the operation</param>
  /// <returns>User DTO containing the created user information</returns>
  /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
  /// <exception cref="DomainException">Thrown when user with email already exists</exception>
  public async Task<UserDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default) {
    if (request == null)
      throw new ArgumentNullException(nameof(request));

    // Check for existing user with same email
    var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
    if (existingUser != null)
      throw new DomainException("User with this email already exists");

    // Create new user with hashed password
    var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
    var user = new User(request.Email, passwordHash, request.FirstName, request.LastName);

    // Persist user and send welcome email
    await _userRepository.AddAsync(user, cancellationToken);
    await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName, cancellationToken);

    return MapToDto(user);
  }

  #endregion

  #region READ Operations

  /// <summary>
  /// Retrieves a user by their unique identifier.
  /// </summary>
  /// <param name="id">Unique identifier of the user</param>
  /// <param name="cancellationToken">Token to cancel the operation</param>
  /// <returns>User DTO if found, otherwise null</returns>
  public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
      var user = await _userRepository.GetByIdAsync(id, cancellationToken);
      return user != null ? MapToDto(user) : null;
  }

  /// <summary>
  /// Retrieves all users from the system.
  /// </summary>
  /// <param name="cancellationToken">Token to cancel the operation</param>
  /// <returns>List of user DTOs containing all users</returns>
  public async Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken = default) {
    var users = await _userRepository.GetAllAsync(cancellationToken);
    return users.Select(MapToDto).ToList();
  }

  #endregion

  #region UPDATE Operations

  /// <summary>
  /// Updates an existing user's information.
  /// </summary>
  /// <param name="request">Update request with user changes</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Updated user DTO or null if not found</returns>
  public async Task<UserDto?> UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken = default)
  {
		var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
		if (user == null) return null;

		user.UpdatePersonalInfo(request.FirstName, request.LastName);

		if (request.IsEmailConfirmed == true)
			user.ConfirmEmail();

		await _userRepository.UpdateAsync(user, cancellationToken);
		return MapToDto(user);
  }

  #endregion

  #region DELETE Operations

  /// <summary>
  /// Deletes a user by their ID.
  /// </summary>
  /// <param name="id">User ID to delete</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>True if deleted, false if not found</returns>
  public async Task<bool> DeleteAsync(DeleteUserRequest request, CancellationToken ct = default)
  {
    if (request is null) throw new ArgumentNullException(nameof(request));
    return await DeleteAsync(request.Id, ct);
  }

  public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
  {
    var user = await _userRepository.GetByIdAsync(id, ct);
    if (user == null) return false;
    await _userRepository.DeleteAsync(user, ct);
    return true;
  }

  #endregion

  #region Authentication Operations

  /// <summary>
  /// Authenticates a user with email and password credentials.
  /// Updates the user's last login timestamp upon successful authentication.
  /// </summary>
  /// <param name="request">Login credentials containing email and password</param>
  /// <param name="cancellationToken">Token to cancel the operation</param>
  /// <returns>User DTO if authentication successful, null if credentials are invalid</returns>
  /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
  public async Task<UserDto?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default) {
    if (request == null)
      throw new ArgumentNullException(nameof(request));

    // Retrieve user and verify password
    var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
    if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
      return null;

    // Update last login timestamp
    user.UpdateLastLogin();
    await _userRepository.UpdateAsync(user, cancellationToken);

    return MapToDto(user);
  }

  #endregion

  #region Password Reset Operations

  /// <summary>
  /// Initiates a password reset process by generating a secure token and sending reset email.
  /// Silently returns if user with email doesn't exist (security best practice).
  /// </summary>
  /// <param name="email">Email address of the user requesting password reset</param>
  /// <param name="cancellationToken">Token to cancel the operation</param>
  /// <returns>A task representing the asynchronous operation</returns>
  /// <exception cref="ArgumentException">Thrown when email is null or empty</exception>
  public async Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default) {
    if (string.IsNullOrWhiteSpace(email))
      throw new ArgumentException("Email cannot be null or empty", nameof(email));

    // Find user by email (return silently if not found for security)
    var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
    if (user == null)
      return;

    // Generate secure token with 1-hour expiry
    var token = GenerateSecureToken();
    var expiry = DateTime.UtcNow.AddHours(1);

    // Set token and send reset email
    user.SetPasswordResetToken(token, expiry);
    await _userRepository.UpdateAsync(user, cancellationToken);
    await _emailService.SendPasswordResetEmailAsync(email, token, cancellationToken);
  }

  /// <summary>
  /// Completes the password reset process using a valid reset token.
  /// Validates token existence and expiry before updating the password.
  /// </summary>
  /// <param name="request">Reset request containing token and new password</param>
  /// <param name="cancellationToken">Token to cancel the operation</param>
  /// <returns>True if password was successfully reset, false if token is invalid or expired</returns>
  /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
  public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default) {
    if (request == null)
      throw new ArgumentNullException(nameof(request));

    // Find user by reset token and check expiry
    var user = await _userRepository.GetByPasswordResetTokenAsync(request.Token, cancellationToken);
    if (user == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
      return false;

    // Update password and clear reset token
    var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
    user.UpdatePassword(passwordHash);
    await _userRepository.UpdateAsync(user, cancellationToken);

    return true;
  }

  #endregion

  #region Private Helper Methods

  /// <summary>
  /// Generates a cryptographically secure random token for password reset operations.
  /// Uses 32 bytes of entropy and URL-safe base64 encoding.
  /// </summary>
  /// <returns>A secure random token string suitable for URLs</returns>
  private static string GenerateSecureToken() {
    var bytes = new byte[32];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(bytes);
    return Convert.ToBase64String(bytes).Replace("/", "_").Replace("+", "-").TrimEnd('=');
  }

  /// <summary>
  /// Maps a User domain entity to a UserDto for external consumption.
  /// </summary>
  /// <param name="user">The user entity to map</param>
  /// <returns>UserDto containing user information</returns>
  private static UserDto MapToDto(User user) => new(user.Id, user.Email, user.FirstName, user.LastName, user.IsEmailConfirmed, user.CreatedAt, user.LastLoginAt);

  #endregion
}
