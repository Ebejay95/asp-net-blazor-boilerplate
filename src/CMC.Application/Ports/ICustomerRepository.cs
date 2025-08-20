using System;
using System.Threading;
using System.Threading.Tasks;
using CMC.Domain.Entities;

namespace CMC.Application.Ports;

/// <summary>
/// Repository interface for managing Customer entities in the data store.
/// Provides CRUD operations and specialized query methods for Customer management.
/// </summary>
public interface ICustomerRepository
{
    // =============================================================================
    // READ Operations
    // =============================================================================

    /// <summary>
    /// Retrieves a Customer by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the Customer</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The Customer if found, otherwise null</returns>
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a Customer by their unique identifier including associated users.
    /// </summary>
    /// <param name="id">The unique identifier of the Customer</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The Customer with users if found, otherwise null</returns>
    Task<Customer?> GetByIdWithUsersAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all Customers from the data store.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A list of all Customers</returns>
    Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all Customers with their associated user counts.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A list of all Customers with user information</returns>
    Task<List<Customer>> GetAllWithUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a Customer by their name.
    /// Used for duplicate name validation.
    /// </summary>
    /// <param name="name">The customer name to search for</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The Customer if found, otherwise null</returns>
    Task<Customer?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves customers by industry sector.
    /// </summary>
    /// <param name="industry">The industry to filter by</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of customers in the specified industry</returns>
    Task<List<Customer>> GetByIndustryAsync(string industry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves active customers only.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of active customers</returns>
    Task<List<Customer>> GetActiveCustomersAsync(CancellationToken cancellationToken = default);

    // =============================================================================
    // CREATE Operations
    // =============================================================================

    /// <summary>
    /// Adds a new Customer to the data store.
    /// </summary>
    /// <param name="customer">The Customer entity to add</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when customer is null</exception>
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);

    // =============================================================================
    // UPDATE Operations
    // =============================================================================

    /// <summary>
    /// Updates an existing Customer in the data store.
    /// </summary>
    /// <param name="customer">The Customer entity with updated values</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when customer is null</exception>
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);

    // =============================================================================
    // DELETE Operations
    // =============================================================================

    /// <summary>
    /// Removes a Customer from the data store.
    /// </summary>
    /// <param name="customer">The Customer entity to remove</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when customer is null</exception>
    Task DeleteAsync(Customer customer, CancellationToken cancellationToken = default);

    // =============================================================================
    // BUSINESS Operations
    // =============================================================================

    /// <summary>
    /// Checks if a customer name already exists in the system.
    /// </summary>
    /// <param name="name">The customer name to check</param>
    /// <param name="excludeId">Optional customer ID to exclude from check (for updates)</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>True if name exists, false otherwise</returns>
    Task<bool> ExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
