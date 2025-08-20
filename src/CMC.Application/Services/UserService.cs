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
/// Handles user registration, authentication, password management, user data retrieval, and customer associations.
/// </summary>
public class UserService {
  #region Fields

  private readonly IUserRepository _userRepository;
  private readonly ICustomerRepository _customerRepository;
  private readonly IEmailService _emailService;

  #endregion

  #region Constructor

  /// <summary>
  /// Initializes a new instance of the UserService class.
  /// </summary>
  /// <param name="userRepository">Repository for user data operations</param>
  /// <param name="customerRepository">Repository for customer data operations</param>
  /// <param name="emailService">Service for sending email notifications</param>
  /// <exception cref="ArgumentNullException">Thrown when any dependency is null</exception>
  public UserService(IUserRepository userRepository, ICustomerRepository customerRepository, IEmailService emailService) {
    _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
  }

  #endregion

  #region CREATE Operations

  /// <summary>
  /// Registers a new user in the system with email verification and welcome notification.
  /// Validates email uniqueness, hashes password securely, and sends welcome email.
  /// Optionally assigns user to a customer if specified.
  /// </summary>
  /// <param name="request">Registration details including email, password, and personal information</param>
  /// <param name="cancellationToken">Token to cancel the operation</param>
  /// <returns>User DTO containing the created user information</returns>
  /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
  /// <exception cref="DomainException">Thrown when user with email already exists or customer not found</exception>
  public async Task<UserDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default) {
    if (request == null)
      throw new ArgumentNullException(nameof(request));

    // Check for existing user with same email
    var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
    if (existingUser != null)
      throw new DomainException("User with this email already exists");

    // Validate customer if specified
    Customer? customer = null;
    if (request.CustomerId.HasValue)
    {
      customer = await _customerRepository.GetByIdAsync(request.CustomerId.Value, cancellationToken);
      if (customer == null)
        throw new DomainException("Specified customer does not exist");
    }

    // Create new user with hashed password
    var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
    var user = new User(request.Email, passwordHash, request.FirstName, request.LastName, request.Role, request.Department);

    // Assign to customer if specified (using direct property assignment)
    if (customer != null)
    {
      user.AssignToCustomer(customer);
    }

    // Persist user and send welcome email
    await _userRepository.AddAsync(user, cancellationToken);
    await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName, cancellationToken);

    return await MapToDtoAsync(user, cancellationToken);
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
      return user != null ? await MapToDtoAsync(user, cancellationToken) : null;
  }

  /// <summary>
  /// Retrieves all users from the system.
  /// </summary>
  /// <param name="cancellationToken">Token to cancel the operation</param>
  /// <returns>List of user DTOs containing all users</returns>
  public async Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken = default) {
    var users = await _userRepository.GetAllAsync(cancellationToken);
    var userDtos = new List<UserDto>();

    foreach (var user in users)
    {
      userDtos.Add(await MapToDtoAsync(user, cancellationToken));
    }

    return userDtos;
  }

  /// <summary>
  /// Retrieves users associated with a specific customer.
  /// </summary>
  /// <param name="customerId">Customer ID to filter by</param>
  /// <param name="cancellationToken">Token to cancel the operation</param>
  /// <returns>List of user DTOs for the specified customer</returns>
  public async Task<List<UserDto>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
  {
    var users = await _userRepository.GetAllAsync(cancellationToken);
    var customerUsers = users.Where(u => u.CustomerId == customerId).ToList();

    var userDtos = new List<UserDto>();
    foreach (var user in customerUsers)
    {
      userDtos.Add(await MapToDtoAsync(user, cancellationToken));
    }

    return userDtos;
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
    if (request == null)
      throw new ArgumentNullException(nameof(request));

    var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
    if (user == null) return null;

    // Update personal information
    user.UpdatePersonalInfo(request.FirstName, request.LastName, request.Role, request.Department);

    // Handle email confirmation
    if (request.IsEmailConfirmed == true)
      user.ConfirmEmail();

    // Handle customer assignment changes
    if (request.CustomerId.HasValue)
    {
      // Remove from current customer if any
      if (user.Customer != null)
      {
        user.RemoveFromCustomer();
      }

      // Assign to new customer
      var newCustomer = await _customerRepository.GetByIdAsync(request.CustomerId.Value, cancellationToken);
      if (newCustomer == null)
        throw new DomainException("Specified customer does not exist");

      user.AssignToCustomer(newCustomer);
    }
    else if (request.CustomerId == null && user.Customer != null)
    {
      // Remove from current customer
      user.RemoveFromCustomer();
    }

    await _userRepository.UpdateAsync(user, cancellationToken);
    return await MapToDtoAsync(user, cancellationToken);
  }

  #endregion

  #region DELETE Operations

  /// <summary>
  /// Deletes a user by their ID.
  /// </summary>
  /// <param name="request">Delete request containing user ID</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>True if deleted, false if not found</returns>
  public async Task<bool> DeleteAsync(DeleteUserRequest request, CancellationToken cancellationToken = default)
  {
    if (request is null) throw new ArgumentNullException(nameof(request));
    return await DeleteAsync(request.Id, cancellationToken);
  }

  /// <summary>
  /// Deletes a user by their ID.
  /// </summary>
  /// <param name="id">User ID to delete</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>True if deleted, false if not found</returns>
  public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var user = await _userRepository.GetByIdAsync(id, cancellationToken);
    if (user == null) return false;

    // Remove from customer if assigned
    if (user.Customer != null)
    {
      user.RemoveFromCustomer();
    }

    await _userRepository.DeleteAsync(user, cancellationToken);
    return true;
  }

  #endregion

  #region Customer Association Operations

  /// <summary>
  /// Assigns a user to a customer.
  /// </summary>
  /// <param name="request">Assignment request containing user and customer IDs</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Updated user DTO or null if user/customer not found</returns>
  public async Task<UserDto?> AssignToCustomerAsync(AssignUserToCustomerRequest request, CancellationToken cancellationToken = default)
  {
    if (request == null)
      throw new ArgumentNullException(nameof(request));

    var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
    if (user == null) return null;

    var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
    if (customer == null)
      throw new DomainException("Customer not found");

    // Remove from current customer if any
    if (user.Customer != null)
    {
      user.RemoveFromCustomer();
    }

    // Assign to new customer
    user.AssignToCustomer(customer);

    await _userRepository.UpdateAsync(user, cancellationToken);
    return await MapToDtoAsync(user, cancellationToken);
  }

  /// <summary>
  /// Removes a user from their current customer.
  /// </summary>
  /// <param name="request">Removal request containing user ID</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Updated user DTO or null if user not found</returns>
  public async Task<UserDto?> RemoveFromCustomerAsync(RemoveUserFromCustomerRequest request, CancellationToken cancellationToken = default)
  {
    if (request == null)
      throw new ArgumentNullException(nameof(request));

    var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
    if (user == null) return null;

    if (user.Customer != null)
    {
      user.RemoveFromCustomer();
      await _userRepository.UpdateAsync(user, cancellationToken);
    }

    return await MapToDtoAsync(user, cancellationToken);
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

    return await MapToDtoAsync(user, cancellationToken);
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
  /// Includes customer information if the user is associated with one.
  /// </summary>
  /// <param name="user">The user entity to map</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>UserDto containing user information</returns>
  private async Task<UserDto> MapToDtoAsync(User user, CancellationToken cancellationToken = default)
  {
    string? customerName = null;

    if (user.CustomerId.HasValue)
    {
      var customer = await _customerRepository.GetByIdAsync(user.CustomerId.Value, cancellationToken);
      customerName = customer?.Name;
    }

    return new UserDto(
      user.Id,
      user.Email,
      user.FirstName,
      user.LastName,
      user.Role,
      user.Department,
      user.IsEmailConfirmed,
      user.CreatedAt,
      user.LastLoginAt,
      user.CustomerId,
      customerName
    );
  }

  #endregion
}
