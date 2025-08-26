using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Domain.ValueObjects;
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
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves all users ordered by creation date with read-only tracking for performance.
    /// </summary>
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
    /// <param name="email">Raw email string; will be normalized by the Email value object.</param>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        // VO erzeugen (trim/validation/normalize), dann direkte VO-Vergleichsoperation.
        var emailVo = (Email)email;
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == emailVo, cancellationToken);
    }

    /// <summary>
    /// Retrieves a user by their password reset token with automatic expiry validation.
    /// Only returns users with valid (non-expired) reset tokens for security.
    /// </summary>
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

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region UPDATE Operations

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region DELETE Operations

    public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion
}
