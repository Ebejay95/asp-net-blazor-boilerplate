using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CMC.Domain.Entities;

namespace CMC.Application.Ports
{
	public interface ICustomerRepository
	{
		// READ
		Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
		Task<Customer?> GetByIdWithUsersAsync(Guid id, CancellationToken cancellationToken = default);
		Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
		Task<List<Customer>> GetAllWithUsersAsync(CancellationToken cancellationToken = default);
		Task<Customer?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
		Task<List<Customer>> GetByIndustryAsync(string industryName, CancellationToken cancellationToken = default);
		Task<List<Customer>> GetActiveCustomersAsync(CancellationToken cancellationToken = default);

		// CUD
		Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
		Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
		Task DeleteAsync(Customer customer, CancellationToken cancellationToken = default);

		// BUSINESS
		Task<bool> ExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
	}
}
