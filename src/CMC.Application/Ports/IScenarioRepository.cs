using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CMC.Domain.Entities;

namespace CMC.Application.Ports
{
	public interface IScenarioRepository
	{
		Task<Scenario?> GetByIdAsync(Guid id, CancellationToken ct = default);
		Task<List<Scenario>> GetAllAsync(CancellationToken ct = default);
		Task<List<Scenario>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);
		Task AddAsync(Scenario scenario, CancellationToken ct = default);
		Task AddRangeAsync(IEnumerable<Scenario> scenarios, CancellationToken ct = default);
		Task UpdateAsync(Scenario scenario, CancellationToken ct = default);
		Task DeleteAsync(Scenario scenario, CancellationToken ct = default);
	}
}
