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
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdWithUsersAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Customer>> GetAllWithUsersAsync(CancellationToken cancellationToken = default);
    Task<Customer?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<Customer>> GetByIndustryAsync(string industry, CancellationToken cancellationToken = default);
    Task<List<Customer>> GetActiveCustomersAsync(CancellationToken cancellationToken = default);

    // =============================================================================
    // CREATE/UPDATE/DELETE
    // =============================================================================
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
    Task DeleteAsync(Customer customer, CancellationToken cancellationToken = default);

    // =============================================================================
    // BUSINESS
    // =============================================================================
    Task<bool> ExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
