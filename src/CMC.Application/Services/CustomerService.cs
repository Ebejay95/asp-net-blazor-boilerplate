using System;
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

			// Industries: upsert + links
			var tokens = ParseIndustryTokens(request.Industry);
			if (tokens.Length > 0)
			{
				var industries = await EnsureIndustriesAsync(tokens, cancellationToken);
				SyncCustomerIndustries(customer, industries);
			}

			await _customerRepository.AddAsync(customer, cancellationToken);
			return MapToReadDto(customer);
		}

		public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			// WICHTIG: Repo sollte IndustryLinks inkl. Industry mitladen!
			var customer = await _customerRepository.GetByIdWithUsersAsync(id, cancellationToken);
			return customer != null ? MapToReadDto(customer) : null;
		}

		public async Task<List<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			// WICHTIG: Repo sollte IndustryLinks inkl. Industry mitladen!
			var customers = await _customerRepository.GetAllWithUsersAsync(cancellationToken);
			return customers.Select(MapToReadDto).ToList();
		}

		public async Task<List<CustomerDto>> GetActiveCustomersAsync(CancellationToken cancellationToken = default)
		{
			// WICHTIG: Repo sollte IndustryLinks inkl. Industry mitladen!
			var customers = await _customerRepository.GetActiveCustomersAsync(cancellationToken);
			return customers.Select(MapToReadDto).ToList();
		}

		public async Task<CustomerDto?> UpdateAsync(UpdateCustomerRequest request, CancellationToken cancellationToken = default)
		{
			if (request == null) throw new ArgumentNullException(nameof(request));

			// WICHTIG: Repo sollte IndustryLinks inkl. Industry mitladen!
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

			// Industries: upsert + links sync
			var tokens = ParseIndustryTokens(request.Industry);
			var industries = await EnsureIndustriesAsync(tokens, cancellationToken);
			SyncCustomerIndustries(customer, industries);

			await _customerRepository.UpdateAsync(customer, cancellationToken);

			// WICHTIG: Repo sollte IndustryLinks inkl. Industry mitladen!
			var updated = await _customerRepository.GetByIdWithUsersAsync(customer.Id, cancellationToken);
			return updated != null ? MapToReadDto(updated) : null;
		}

		public async Task<bool> DeleteAsync(DeleteCustomerRequest request, CancellationToken cancellationToken = default)
		{
			if (request is null) throw new ArgumentNullException(nameof(request));
			return await DeleteAsync(request.Id, cancellationToken);
		}

		public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
		{
			// WICHTIG: Repo sollte Users (und idealerweise IndustryLinks) mitladen
			var customer = await _customerRepository.GetByIdWithUsersAsync(id, cancellationToken);
			if (customer == null) return false;
			if (customer.Users.Any()) throw new DomainException("Cannot delete customer with associated users. Please remove all users first.");

			await _customerRepository.DeleteAsync(customer, cancellationToken);
			return true;
		}

		// -------------------- Helpers --------------------

		private static DateTime ToUtc(DateTimeOffset dto) => dto.UtcDateTime;

		private static string BuildIndustryString(Customer c)
		{
			var names = c.IndustryLinks?
				.Select(l => l.Industry?.Name)
				.Where(n => !string.IsNullOrWhiteSpace(n))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.OrderBy(n => n)
				.ToArray();

			return (names == null || names.Length == 0) ? string.Empty : string.Join(", ", names);
		}

		private static CustomerDto MapToReadDto(Customer c) => new(
			Id: c.Id,
			Name: c.Name,
			Industry: BuildIndustryString(c),
			EmployeeCount: c.EmployeeCount,
			RevenuePerYear: c.RevenuePerYear,
			IsActive: c.IsActive,
			CreatedAt: ToUtc(c.CreatedAt),
			UpdatedAt: ToUtc(c.UpdatedAt),
			UserCount: c.Users?.Count ?? 0
		);

		private static string[] ParseIndustryTokens(string? raw)
		{
			if (string.IsNullOrWhiteSpace(raw)) return Array.Empty<string>();
			char[] seps = new[] { ',', ';', '|', '/' };
			return raw
				.Split(seps, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.Select(s => s.Trim())
				.Where(s => s.Length > 0)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToArray();
		}

		private async Task<List<Industry>> EnsureIndustriesAsync(string[] tokens, CancellationToken ct)
		{
			var result = new List<Industry>(tokens.Length);
			foreach (var name in tokens)
			{
				var existing = await _industryRepository.GetByNameAsync(name, ct);
				if (existing == null)
				{
					existing = new Industry(name);
					await _industryRepository.AddAsync(existing, ct);
				}
				result.Add(existing);
			}
			return result;
		}

		private static void SyncCustomerIndustries(Customer c, IReadOnlyCollection<Industry> target)
		{
			var targetIds = target.Select(i => i.Id).ToHashSet();
			var toRemove = c.IndustryLinks.Where(l => !targetIds.Contains(l.IndustryId)).ToList();
			foreach (var r in toRemove) c.IndustryLinks.Remove(r);

			var existingIds = c.IndustryLinks.Select(l => l.IndustryId).ToHashSet();
			foreach (var ind in target)
			{
				if (!existingIds.Contains(ind.Id))
					c.IndustryLinks.Add(new CustomerIndustry(c.Id, ind.Id));
			}
		}
	}
}
