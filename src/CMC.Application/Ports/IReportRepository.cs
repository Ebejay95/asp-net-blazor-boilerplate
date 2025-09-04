using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CMC.Domain.Entities;

namespace CMC.Application.Ports
{
	public interface IReportRepository
	{
		Task<Report?> GetByIdAsync(Guid id, CancellationToken ct = default);
		Task<List<Report>> GetAllAsync(CancellationToken ct = default);
		Task<List<Report>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);

		Task AddAsync(Report entity, CancellationToken ct = default);
		Task UpdateAsync(Report entity, CancellationToken ct = default);
		Task DeleteAsync(Report entity, CancellationToken ct = default);
	}
}
