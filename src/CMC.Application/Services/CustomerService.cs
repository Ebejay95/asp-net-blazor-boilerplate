using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Contracts.Customers;
using CMC.Domain.Common;
using CMC.Domain.Entities;

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

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }

    #endregion

    #region CREATE Operations

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var existingCustomer = await _customerRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingCustomer != null)
            throw new DomainException("Customer with this name already exists");

        var customer = new Customer(request.Name, request.Industry, request.EmployeeCount, request.RevenuePerYear);

        await _customerRepository.AddAsync(customer, cancellationToken);

        return MapToReadDto(customer);
    }

    #endregion

    #region READ Operations

    public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdWithUsersAsync(id, cancellationToken);
        return customer != null ? MapToReadDto(customer) : null;
    }

    public async Task<List<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetAllWithUsersAsync(cancellationToken);
        return customers.Select(MapToReadDto).ToList();
    }

    public async Task<List<CustomerDto>> GetActiveCustomersAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetActiveCustomersAsync(cancellationToken);
        return customers.Select(MapToReadDto).ToList();
    }

    public async Task<List<CustomerDto>> GetByIndustryAsync(string industry, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(industry))
            throw new ArgumentException("Industry cannot be null or empty", nameof(industry));

        var customers = await _customerRepository.GetByIndustryAsync(industry, cancellationToken);
        return customers.Select(MapToReadDto).ToList();
    }

    #endregion

    #region UPDATE Operations

    public async Task<CustomerDto?> UpdateAsync(UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null) return null;

        if (await _customerRepository.ExistsAsync(request.Name, request.Id, cancellationToken))
            throw new DomainException("Customer with this name already exists");

        customer.UpdateBusinessInfo(request.Name, request.Industry, request.EmployeeCount, request.RevenuePerYear);

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value && !customer.IsActive)
                customer.Activate();
            else if (!request.IsActive.Value && customer.IsActive)
                customer.Deactivate();
        }

        await _customerRepository.UpdateAsync(customer, cancellationToken);

        var updatedCustomer = await _customerRepository.GetByIdWithUsersAsync(customer.Id, cancellationToken);
        return updatedCustomer != null ? MapToReadDto(updatedCustomer) : null;
    }

    #endregion

    #region DELETE Operations

    public async Task<bool> DeleteAsync(DeleteCustomerRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        return await DeleteAsync(request.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdWithUsersAsync(id, cancellationToken);
        if (customer == null) return false;

        if (customer.Users.Any())
            throw new DomainException("Cannot delete customer with associated users. Please remove all users first.");

        await _customerRepository.DeleteAsync(customer, cancellationToken);
        return true;
    }

    #endregion

    #region Private Helper Methods

    // --- Time conversions: domain may use DateTime or DateTimeOffset; DTO expects DateTime (UTC) ---
    private static DateTime ToUtc(DateTime dt) => dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime();
    private static DateTime? ToUtc(DateTime? dt) => dt.HasValue ? ToUtc(dt.Value) : (DateTime?)null;
    private static DateTime ToUtc(DateTimeOffset dto) => dto.UtcDateTime;
    private static DateTime? ToUtc(DateTimeOffset? dto) => dto.HasValue ? dto.Value.UtcDateTime : (DateTime?)null;

    private static CustomerDto MapToReadDto(Customer customer) => new(
        customer.Id,
        customer.Name,
        customer.Industry,
        customer.EmployeeCount,
        customer.RevenuePerYear,
        customer.IsActive,
        ToUtc(customer.CreatedAt),
        ToUtc(customer.UpdatedAt),
        customer.Users?.Count ?? 0
    );

    #endregion
}
