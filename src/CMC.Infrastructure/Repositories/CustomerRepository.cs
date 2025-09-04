using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories
{
	public class CustomerRepository : ICustomerRepository
	{
		private readonly AppDbContext _context;
		public CustomerRepository(AppDbContext context) => _context = context ?? throw new ArgumentNullException(nameof(context));

		public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
			=> await _context.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        public async Task<Customer?> GetByIdWithUsersAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Customers
                .Include(c => c.Users)
                .Include(c => c.CustomerIndustries)
                    .ThenInclude(ci => ci.Industry)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

		public async Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
			=> await _context.Customers
				.AsNoTracking()
				.OrderBy(c => c.Name)
				.ToListAsync(cancellationToken);

        public async Task<List<Customer>> GetAllWithUsersAsync(CancellationToken cancellationToken = default)
            => await _context.Customers
                .Include(c => c.Users)
                .Include(c => c.CustomerIndustries)
                    .ThenInclude(ci => ci.Industry)
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);

		public async Task<Customer?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be null or empty", nameof(name));
			return await _context.Customers.FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
		}

        public async Task<List<Customer>> GetByIndustryAsync(string industryName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(industryName)) throw new ArgumentException(nameof(industryName));

            return await _context.Customers
                .Include(c => c.CustomerIndustries).ThenInclude(ci => ci.Industry)
                .Where(c => c.CustomerIndustries.Any(ci => ci.Industry.Name == industryName))
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Customer>> GetActiveCustomersAsync(CancellationToken cancellationToken = default)
            => await _context.Customers
                .Where(c => c.IsActive)
                .Include(c => c.CustomerIndustries).ThenInclude(ci => ci.Industry)
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);

		public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
		{
			if (customer == null) throw new ArgumentNullException(nameof(customer));
			_context.Customers.Add(customer);
			await _context.SaveChangesAsync(cancellationToken);
		}

		public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
		{
			if (customer == null) throw new ArgumentNullException(nameof(customer));
			_context.Customers.Update(customer);
			await _context.SaveChangesAsync(cancellationToken);
		}

		public async Task DeleteAsync(Customer customer, CancellationToken cancellationToken = default)
		{
			if (customer == null) throw new ArgumentNullException(nameof(customer));
			_context.Customers.Remove(customer);
			await _context.SaveChangesAsync(cancellationToken);
		}

		public async Task<bool> ExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be null or empty", nameof(name));
			var query = _context.Customers.Where(c => c.Name == name);
			if (excludeId.HasValue) query = query.Where(c => c.Id != excludeId.Value);
			return await query.AnyAsync(cancellationToken);
		}
	}
}
