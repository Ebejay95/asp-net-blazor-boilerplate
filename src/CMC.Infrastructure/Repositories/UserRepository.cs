using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories;

public class UserRepository: IUserRepository {
  private readonly AppDbContext _context;

  public UserRepository(AppDbContext context) {
    _context = context;
  }

  public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) {
    return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
  }

  public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) {
    return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
  }

  public async Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default) {
    return await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token && u.PasswordResetTokenExpiry > DateTime.UtcNow, cancellationToken);
  }

  public async Task AddAsync(User user, CancellationToken cancellationToken = default) {
    _context.Users.Add(user);
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task UpdateAsync(User user, CancellationToken cancellationToken = default) {
    _context.Users.Update(user);
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task DeleteAsync(User user, CancellationToken cancellationToken = default) {
    _context.Users.Remove(user);
    await _context.SaveChangesAsync(cancellationToken);
  }
}
