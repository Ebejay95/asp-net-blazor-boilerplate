using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Contracts.Customers;
using CMC.Domain.Common;
using CMC.Domain.Entities;

namespace CMC.Application.Services
{
    public class CustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IIndustryRepository _industryRepository;

        public CustomerService(ICustomerRepository customerRepository, IIndustryRepository industryRepository)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _industryRepository = industryRepository ?? throw new ArgumentNullException(nameof(industryRepository));
        }

        public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var existing = await _customerRepository.GetByNameAsync(request.Name, cancellationToken);
            if (existing != null) throw new DomainException("Customer with this name already exists");

            var customer = new Customer(request.Name, request.EmployeeCount, request.RevenuePerYear);

            // Industries laden & validieren (optional)
            var ids = (request.IndustryIds ?? Array.Empty<Guid>()).Where(x => x != Guid.Empty).Distinct().ToArray();
            if (ids.Length > 0)
            {
                var industries = await _industryRepository.GetByIdsAsync(ids, cancellationToken);
                if (industries.Count != ids.Length)
                    throw new DomainException("One or more specified industries do not exist");

                // neue API: nur IDs setzen (Join-Entities werden in der Domain erzeugt)
                customer.SetIndustries(ids);
            }

            await _customerRepository.AddAsync(customer, cancellationToken);

            var persisted = await _customerRepository.GetByIdWithUsersAsync(customer.Id, cancellationToken) ?? customer;
            return MapToReadDto(persisted);
        }

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

        public async Task<CustomerDto?> UpdateAsync(UpdateCustomerRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (customer == null) return null;

            if (await _customerRepository.ExistsAsync(request.Name, request.Id, cancellationToken))
                throw new DomainException("Customer with this name already exists");

            customer.UpdateBusinessInfo(request.Name, request.EmployeeCount, request.RevenuePerYear);

            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value && !customer.IsActive) customer.Activate();
                if (!request.IsActive.Value && customer.IsActive) customer.Deactivate();
            }

            // Industries: NULL = keine Änderung, leere Liste = alle entfernen
            if (request.IndustryIds != null)
            {
                var ids = request.IndustryIds.Where(x => x != Guid.Empty).Distinct().ToArray();
                var industries = ids.Length == 0
                    ? new List<Industry>()
                    : await _industryRepository.GetByIdsAsync(ids, cancellationToken);

                if (industries.Count != ids.Length)
                    throw new DomainException("One or more specified industries do not exist");

                customer.SetIndustries(ids);
            }

            await _customerRepository.UpdateAsync(customer, cancellationToken);
            var updated = await _customerRepository.GetByIdWithUsersAsync(customer.Id, cancellationToken);
            return updated != null ? MapToReadDto(updated) : MapToReadDto(customer);
        }

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

        // -------------------- Helpers --------------------
        private static CustomerDto MapToReadDto(Customer c)
        {
            // neue Join-Nav verwenden
            var industryIds = c.CustomerIndustries?.Select(ci => ci.IndustryId).Distinct().ToArray() ?? Array.Empty<Guid>();
            var industryNames = c.CustomerIndustries?
                .Select(ci => ci.Industry?.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Cast<string>()
                .Distinct()
                .ToArray() ?? Array.Empty<string>();

            return new CustomerDto(
                Id: c.Id,
                Name: c.Name,
                IndustryIds: industryIds,
                IndustryNames: industryNames,
                EmployeeCount: c.EmployeeCount,
                RevenuePerYear: c.RevenuePerYear,
                IsActive: c.IsActive,
                CreatedAt: c.CreatedAt,
                UpdatedAt: c.UpdatedAt,
                UserCount: c.Users?.Count ?? 0
            );
        }
    }
}
