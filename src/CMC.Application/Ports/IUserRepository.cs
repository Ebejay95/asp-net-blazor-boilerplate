using System;
using System.Threading;
using System.Threading.Tasks;
using CMC.Domain.Entities;

namespace CMC.Application.Ports;

public interface IUserRepository {
  Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
  Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);
  Task AddAsync(User user, CancellationToken cancellationToken = default);
  Task UpdateAsync(User user, CancellationToken cancellationToken = default);
  Task DeleteAsync(User user, CancellationToken cancellationToken = default);
}
