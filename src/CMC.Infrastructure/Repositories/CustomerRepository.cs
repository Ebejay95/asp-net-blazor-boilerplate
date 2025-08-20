using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of the Customer repository.
/// Provides data access operations for Customer entities using EF Core.
/// Implements the Repository pattern to abstract database operations from the application layer.
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    #region Fields

    private readonly AppDbContext _context;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the CustomerRepository with the specified database context.
    /// </summary>
    /// <param name="context">The Entity Framework database context</param>
    /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
    public CustomerRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #endregion

    #region READ Operations

    /// <summary>
    /// Retrieves a customer by their unique identifier with change tracking enabled.
    /// </summary>
    /// <param name="id">The unique identifier of the customer</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The customer if found, otherwise null</returns>
    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves a customer by their unique identifier including associated users.
    /// </summary>
    /// <param name="id">The unique identifier of the customer</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The customer with users if found, otherwise null</returns>
    public async Task<Customer?> GetByIdWithUsersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves all customers ordered by creation date with read-only tracking for performance.
    /// Uses AsNoTracking for better performance when entities won't be modified.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A list of all customers ordered by creation date</returns>
    public async Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves all customers with their associated user counts.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A list of all customers with user information</returns>
    public async Task<List<Customer>> GetAllWithUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Include(c => c.Users)
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a customer by their name for duplicate validation.
    /// Customer names should be unique in the system.
    /// </summary>
    /// <param name="name">The customer name to search for</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The customer if found, otherwise null</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty</exception>
    public async Task<Customer?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }

    /// <summary>
    /// Retrieves customers by industry sector.
    /// </summary>
    /// <param name="industry">The industry to filter by</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of customers in the specified industry</returns>
    /// <exception cref="ArgumentException">Thrown when industry is null or empty</exception>
    public async Task<List<Customer>> GetByIndustryAsync(string industry, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(industry))
            throw new ArgumentException("Industry cannot be null or empty", nameof(industry));

        return await _context.Customers
            .Where(c => c.Industry == industry)
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves active customers only.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of active customers</returns>
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

    /// <summary>
    /// Adds a new customer to the database and immediately saves changes.
    /// Commits the transaction to ensure the customer is persisted.
    /// </summary>
    /// <param name="customer">The customer entity to add</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when customer is null</exception>
    /// <exception cref="DbUpdateException">Thrown when database constraints are violated (e.g., duplicate name)</exception>
    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        if (customer == null) throw new ArgumentNullException(nameof(customer));

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region UPDATE Operations

    /// <summary>
    /// Updates an existing customer in the database and immediately saves changes.
    /// Marks the entity as modified and commits the transaction.
    /// </summary>
    /// <param name="customer">The customer entity with updated values</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when customer is null</exception>
    /// <exception cref="DbUpdateConcurrencyException">Thrown when the entity has been modified by another process</exception>
    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        if (customer == null) throw new ArgumentNullException(nameof(customer));

        _context.Customers.Update(customer);
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region DELETE Operations

    /// <summary>
    /// Removes a customer from the database and immediately saves changes.
    /// Permanently deletes the customer entity from the data store.
    /// </summary>
    /// <param name="customer">The customer entity to remove</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when customer is null</exception>
    /// <exception cref="DbUpdateException">Thrown when foreign key constraints prevent deletion</exception>
    public async Task DeleteAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        if (customer == null) throw new ArgumentNullException(nameof(customer));

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region BUSINESS Operations

    /// <summary>
    /// Checks if a customer name already exists in the system.
    /// </summary>
    /// <param name="name">The customer name to check</param>
    /// <param name="excludeId">Optional customer ID to exclude from check (for updates)</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>True if name exists, false otherwise</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty</exception>
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
