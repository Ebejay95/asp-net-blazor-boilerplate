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
  // READ
  Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
  Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
  Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);

  // CREATE
  Task AddAsync(User user, CancellationToken cancellationToken = default);

  // UPDATE
  Task UpdateAsync(User user, CancellationToken cancellationToken = default);

  // DELETE
  Task DeleteAsync(User user, CancellationToken cancellationToken = default);
}
