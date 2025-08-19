using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of the User repository.
/// Provides data access operations for User entities using EF Core.
/// Implements the Repository pattern to abstract database operations from the application layer.
/// </summary>
public class UserRepository : IUserRepository
{
    #region Fields

    private readonly AppDbContext _context;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the UserRepository with the specified database context.
    /// </summary>
    /// <param name="context">The Entity Framework database context</param>
    /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
    public UserRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #endregion

    #region READ Operations

    /// <summary>
    /// Retrieves a user by their unique identifier with change tracking enabled.
    /// </summary>
    /// <param name="id">The unique identifier of the user</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The user if found, otherwise null</returns>
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves all users ordered by creation date with read-only tracking for performance.
    /// Uses AsNoTracking for better performance when entities won't be modified.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A list of all users ordered by creation date</returns>
    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a user by their email address for authentication purposes.
    /// Email is unique in the system, so this will return at most one user.
    /// </summary>
    /// <param name="email">The email address to search for</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The user if found, otherwise null</returns>
    /// <exception cref="ArgumentException">Thrown when email is null or empty</exception>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    /// <summary>
    /// Retrieves a user by their password reset token with automatic expiry validation.
    /// Only returns users with valid (non-expired) reset tokens for security.
    /// </summary>
    /// <param name="token">The password reset token to search for</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The user if found with a valid token, otherwise null</returns>
    /// <exception cref="ArgumentException">Thrown when token is null or empty</exception>
    public async Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));

        return await _context.Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token &&
                                     u.PasswordResetTokenExpiry > DateTime.UtcNow,
                                cancellationToken);
    }

    #endregion

    #region CREATE Operations

    /// <summary>
    /// Adds a new user to the database and immediately saves changes.
    /// Commits the transaction to ensure the user is persisted.
    /// </summary>
    /// <param name="user">The user entity to add</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null</exception>
    /// <exception cref="DbUpdateException">Thrown when database constraints are violated (e.g., duplicate email)</exception>
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region UPDATE Operations

    /// <summary>
    /// Updates an existing user in the database and immediately saves changes.
    /// Marks the entity as modified and commits the transaction.
    /// </summary>
    /// <param name="user">The user entity with updated values</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null</exception>
    /// <exception cref="DbUpdateConcurrencyException">Thrown when the entity has been modified by another process</exception>
    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region DELETE Operations

    /// <summary>
    /// Removes a user from the database and immediately saves changes.
    /// Permanently deletes the user entity from the data store.
    /// </summary>
    /// <param name="user">The user entity to remove</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null</exception>
    /// <exception cref="DbUpdateException">Thrown when foreign key constraints prevent deletion</exception>
    public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion
}
