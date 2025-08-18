using System;
using System.Threading;
using System.Threading.Tasks;
using CMC.Domain.Entities;

namespace CMC.Application.Ports;

/// <summary>
/// Repository interface for managing User entities in the data store.
/// Provides CRUD operations and specialized query methods for user management.
/// </summary>
public interface IUserRepository {
  // =============================================================================
  // READ Operations
  // =============================================================================

  /// <summary>
  /// Retrieves a user by their unique identifier.
  /// </summary>
  /// <param name="id">The unique identifier of the user</param>
  /// <param name="cancellationToken">Token to cancel the operation</param>
  /// <returns>The user if found, otherwise null</returns>
  Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

  /// <summary>
  /// Retrieves all users from the data store.
  /// </summary>
  /// <param name="cancellationToken">Token to cancel the operation</param>
  /// <returns>A list of all users</returns>
  Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Retrieves a user by their email address.
  /// Used primarily for authentication and duplicate email validation.
  /// </summary>
  /// <param name="email">The email address to search for</param>
  /// <param name="cancellationToken">Token to cancel the operation</param>
  /// <returns>The user if found, otherwise null</returns>
  Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

  /// <summary>
  /// Retrieves a user by their password reset token.
  /// Used during the password reset process to validate tokens.
  /// </summary>
  /// <param name="token">The password reset token to search for</param>
  /// <param name="cancellationToken">Token to cancel the operation</param>
  /// <returns>The user if found with a valid token, otherwise null</returns>
  Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);

  // =============================================================================
  // CREATE Operations
  // =============================================================================

  /// <summary>
  /// Adds a new user to the data store.
  /// </summary>
  /// <param name="user">The user entity to add</param>
  /// <param name="cancellationToken">Token to cancel the operation</param>
  /// <returns>A task representing the asynchronous operation</returns>
  /// <exception cref="ArgumentNullException">Thrown when user is null</exception>
  Task AddAsync(User user, CancellationToken cancellationToken = default);

  // =============================================================================
  // UPDATE Operations
  // =============================================================================

  /// <summary>
  /// Updates an existing user in the data store.
  /// </summary>
  /// <param name="user">The user entity with updated values</param>
  /// <param name="cancellationToken">Token to cancel the operation</param>
  /// <returns>A task representing the asynchronous operation</returns>
  /// <exception cref="ArgumentNullException">Thrown when user is null</exception>
  Task UpdateAsync(User user, CancellationToken cancellationToken = default);

  // =============================================================================
  // DELETE Operations
  // =============================================================================

  /// <summary>
  /// Removes a user from the data store.
  /// </summary>
  /// <param name="user">The user entity to remove</param>
  /// <param name="cancellationToken">Token to cancel the operation</param>
  /// <returns>A task representing the asynchronous operation</returns>
  /// <exception cref="ArgumentNullException">Thrown when user is null</exception>
  Task DeleteAsync(User user, CancellationToken cancellationToken = default);
}
