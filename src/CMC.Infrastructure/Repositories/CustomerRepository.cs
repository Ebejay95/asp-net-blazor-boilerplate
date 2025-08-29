using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of the Customer repository.
/// </summary>
public class CustomerRepository : ICustomerRepository
{
	#region Fields
	private readonly AppDbContext _context;
	#endregion

	#region Constructor
	public CustomerRepository(AppDbContext context)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
	}
	#endregion

	#region READ Operations
	public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _context.Customers
			.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
	}

	public async Task<Customer?> GetByIdWithUsersAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _context.Customers
			.Include(c => c.Users)
			.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
	}

	public async Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await _context.Customers
			.AsNoTracking()
			.OrderBy(c => c.Name)
			.ToListAsync(cancellationToken);
	}

	public async Task<List<Customer>> GetAllWithUsersAsync(CancellationToken cancellationToken = default)
	{
		return await _context.Customers
			.Include(c => c.Users)
			.AsNoTracking()
			.OrderBy(c => c.Name)
			.ToListAsync(cancellationToken);
	}

	public async Task<Customer?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Name cannot be null or empty", nameof(name));

		return await _context.Customers
			.FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
	}

	/// <summary>
	/// NEU: Filter über Join-Tabelle CustomerIndustries (Customer hat kein direktes Industry-Property mehr).
	/// </summary>
	public async Task<List<Customer>> GetByIndustryAsync(string industry, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(industry))
			throw new ArgumentException("Industry cannot be null or empty", nameof(industry));

		var industryIds = _context.Industries
			.Where(i => i.Name == industry)
			.Select(i => i.Id);

		return await _context.CustomerIndustries
			.Where(ci => industryIds.Contains(ci.IndustryId))
			.Select(ci => ci.Customer!)
			.AsNoTracking()
			.OrderBy(c => c.Name)
			.ToListAsync(cancellationToken);
	}

	public async Task<List<Customer>> GetActiveCustomersAsync(CancellationToken cancellationToken = default)
	{
		return await _context.Customers
			.Where(c => c.IsActive)
			.AsNoTracking()
			.OrderBy(c => c.Name)
			.ToListAsync(cancellationToken);
	}
	#endregion

	#region CREATE Operations
	public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
	{
		if (customer == null) throw new ArgumentNullException(nameof(customer));
		_context.Customers.Add(customer);
		await _context.SaveChangesAsync(cancellationToken);
	}
	#endregion

	#region UPDATE Operations
	public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
	{
		if (customer == null) throw new ArgumentNullException(nameof(customer));
		_context.Customers.Update(customer);
		await _context.SaveChangesAsync(cancellationToken);
	}
	#endregion

	#region DELETE Operations
	public async Task DeleteAsync(Customer customer, CancellationToken cancellationToken = default)
	{
		if (customer == null) throw new ArgumentNullException(nameof(customer));
		_context.Customers.Remove(customer);
		await _context.SaveChangesAsync(cancellationToken);
	}
	#endregion

	#region BUSINESS Operations
	public async Task<bool> ExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Name cannot be null or empty", nameof(name));

		var query = _context.Customers.Where(c => c.Name == name);
		if (excludeId.HasValue)
			query = query.Where(c => c.Id != excludeId.Value);

		return await query.AnyAsync(cancellationToken);
	}
	#endregion
}
