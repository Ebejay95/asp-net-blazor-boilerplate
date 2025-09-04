using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CMC.Domain.Entities;

namespace CMC.Application.Ports
{
	public interface IReportDefinitionRepository
	{
		Task<ReportDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default);
		Task<List<ReportDefinition>> GetAllAsync(CancellationToken ct = default);
		Task<List<ReportDefinition>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);

		Task AddAsync(ReportDefinition entity, CancellationToken ct = default);
		Task UpdateAsync(ReportDefinition entity, CancellationToken ct = default);
		Task DeleteAsync(ReportDefinition entity, CancellationToken ct = default);
	}
}
