using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CMC.Domain.Entities;

namespace CMC.Application.Ports
{
	public interface IRiskAcceptanceRepository
	{
		Task<RiskAcceptance?> GetByIdAsync(Guid id, CancellationToken ct = default);
		Task<List<RiskAcceptance>> GetAllAsync(CancellationToken ct = default);
		Task<List<RiskAcceptance>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);
		Task<List<RiskAcceptance>> GetByControlAsync(Guid controlId, CancellationToken ct = default);
		Task<List<RiskAcceptance>> GetActiveByControlAsync(Guid controlId, DateTimeOffset asOfUtc, CancellationToken ct = default);

		Task AddAsync(RiskAcceptance entity, CancellationToken ct = default);
		Task UpdateAsync(RiskAcceptance entity, CancellationToken ct = default);
		Task DeleteAsync(RiskAcceptance entity, CancellationToken ct = default);
	}
}
