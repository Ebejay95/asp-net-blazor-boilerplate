using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CMC.Domain.Entities;

namespace CMC.Application.Ports
{
	public interface IEvidenceRepository
	{
		Task<Evidence?> GetByIdAsync(Guid id, CancellationToken ct = default);
		Task<List<Evidence>> GetAllAsync(CancellationToken ct = default);
		Task<List<Evidence>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);
		Task AddAsync(Evidence evidence, CancellationToken ct = default);
		Task UpdateAsync(Evidence evidence, CancellationToken ct = default);
		Task DeleteAsync(Evidence evidence, CancellationToken ct = default);
	}
}
