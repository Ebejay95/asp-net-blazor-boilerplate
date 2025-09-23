using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Domain.ValueObjects;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the User repository.
    /// Provides data access operations for User entities using EF Core.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // READ

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
            => _context.Users
                .AsNoTracking()
                .OrderBy(u => u.CreatedAt)
                .ToListAsync(cancellationToken);

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            var emailVo = (Email)email;
            return _context.Users.FirstOrDefaultAsync(u => u.Email == emailVo, cancellationToken);
        }

        public Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be null or empty", nameof(token));

            return _context.Users.FirstOrDefaultAsync(
                u => u.PasswordResetToken == token &&
                    u.PasswordResetTokenExpiry > DateTimeOffset.UtcNow,
                cancellationToken);
        }

        public Task<List<User>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            if (customerId == Guid.Empty)
                throw new ArgumentException(nameof(customerId));

            return _context.Users
                .AsNoTracking()
                .Where(u => u.CustomerId == customerId)
                .OrderBy(u => u.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        // CREATE

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // UPDATE

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // DELETE

        public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
        }
        // NEUE METHODE: Frische DB-Abfrage ohne EF Change Tracking Cache
        public async Task<User?> GetByEmailFreshAsync(string email, CancellationToken cancellationToken = default)
        {
            // AsNoTracking() umgeht den EF Change Tracker Cache
            // Reload() würde den Cache aktualisieren, aber AsNoTracking ist sauberer
            return await _context.Users
                .AsNoTracking() // Wichtig: Cache-Bypass
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }
    }
}
