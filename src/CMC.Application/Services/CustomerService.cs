using System;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Contracts.Customers;
using CMC.Domain.Common;
using CMC.Domain.Entities;
using System.Linq;

namespace CMC.Application.Services;

/// <summary>
/// Application service for managing Customer-related business operations.
/// Handles Customer registration, updates, deletion, and Customer data retrieval.
/// </summary>
public class CustomerService
{
    #region Fields

    private readonly ICustomerRepository _customerRepository;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the CustomerService class.
    /// </summary>
    /// <param name="customerRepository">Repository for Customer data operations</param>
    /// <exception cref="ArgumentNullException">Thrown when any dependency is null</exception>
    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }

    #endregion

    #region CREATE Operations

    /// <summary>
    /// Creates a new customer in the system.
    /// Validates business rules and ensures name uniqueness.
    /// </summary>
    /// <param name="request">Customer creation details</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Customer DTO containing the created customer information</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    /// <exception cref="DomainException">Thrown when customer with name already exists</exception>
    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Check for existing customer with same name
        var existingCustomer = await _customerRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingCustomer != null)
            throw new DomainException("Customer with this name already exists");

        // Create new customer
        var customer = new Customer(request.Name, request.Industry, request.EmployeeCount, request.RevenuePerYear);

        // Persist customer
        await _customerRepository.AddAsync(customer, cancellationToken);

        return MapToDto(customer);
    }

    #endregion

    #region READ Operations

    /// <summary>
    /// Retrieves a Customer by their unique identifier.
    /// </summary>
    /// <param name="id">Unique identifier of the Customer</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Customer DTO if found, otherwise null</returns>
    public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdWithUsersAsync(id, cancellationToken);
        return customer != null ? MapToDto(customer) : null;
    }

    /// <summary>
    /// Retrieves all Customers from the system.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of Customer DTOs containing all Customers</returns>
    public async Task<List<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetAllWithUsersAsync(cancellationToken);
        return customers.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Retrieves active customers only.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of active customer DTOs</returns>
    public async Task<List<CustomerDto>> GetActiveCustomersAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetActiveCustomersAsync(cancellationToken);
        return customers.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Retrieves customers by industry.
    /// </summary>
    /// <param name="industry">Industry to filter by</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of customer DTOs in the specified industry</returns>
    public async Task<List<CustomerDto>> GetByIndustryAsync(string industry, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(industry))
            throw new ArgumentException("Industry cannot be null or empty", nameof(industry));

        var customers = await _customerRepository.GetByIndustryAsync(industry, cancellationToken);
        return customers.Select(MapToDto).ToList();
    }

    #endregion

    #region UPDATE Operations

    /// <summary>
    /// Updates an existing Customer's information.
    /// </summary>
    /// <param name="request">Update request with Customer changes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated Customer DTO or null if not found</returns>
    public async Task<CustomerDto?> UpdateAsync(UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null) return null;

        // Check for name conflicts (excluding current customer)
        if (await _customerRepository.ExistsAsync(request.Name, request.Id, cancellationToken))
            throw new DomainException("Customer with this name already exists");

        // Update business information
        customer.UpdateBusinessInfo(request.Name, request.Industry, request.EmployeeCount, request.RevenuePerYear);

        // Handle status changes if provided
        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value && !customer.IsActive)
                customer.Activate();
            else if (!request.IsActive.Value && customer.IsActive)
                customer.Deactivate();
        }

        await _customerRepository.UpdateAsync(customer, cancellationToken);

        // Return updated customer with user count
        var updatedCustomer = await _customerRepository.GetByIdWithUsersAsync(customer.Id, cancellationToken);
        return updatedCustomer != null ? MapToDto(updatedCustomer) : null;
    }

    #endregion

    #region DELETE Operations

    /// <summary>
    /// Deletes a Customer by their ID.
    /// </summary>
    /// <param name="request">Delete request containing customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    public async Task<bool> DeleteAsync(DeleteCustomerRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        return await DeleteAsync(request.Id, cancellationToken);
    }

    /// <summary>
    /// Deletes a Customer by their ID.
    /// </summary>
    /// <param name="id">Customer ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdWithUsersAsync(id, cancellationToken);
        if (customer == null) return false;

        // Check if customer has associated users
        if (customer.Users.Any())
            throw new DomainException("Cannot delete customer with associated users. Please remove all users first.");

        await _customerRepository.DeleteAsync(customer, cancellationToken);
        return true;
    }

    #endregion

    #region Business Operations

    /// <summary>
    /// Checks if a customer name is available.
    /// </summary>
    /// <param name="name">Name to check</param>
    /// <param name="excludeId">Optional customer ID to exclude from check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if name is available, false if already taken</returns>
    public async Task<bool> IsNameAvailableAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        return !await _customerRepository.ExistsAsync(name, excludeId, cancellationToken);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Maps a Customer domain entity to a CustomerDto for external consumption.
    /// </summary>
    /// <param name="customer">The customer entity to map</param>
    /// <returns>CustomerDto containing customer information</returns>
    private static CustomerDto MapToDto(Customer customer) => new(
        customer.Id,
        customer.Name,
        customer.Industry,
        customer.EmployeeCount,
        customer.RevenuePerYear,
        customer.IsActive,
        customer.CreatedAt,
        customer.UpdatedAt,
        customer.Users?.Count ?? 0
    );

    #endregion
}
