using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CMC.Domain.Entities;

namespace CMC.Application.Ports
{
	public interface IIndustryRepository
	{
        Task<Industry?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Industry?> GetByNameAsync(string name, CancellationToken ct = default);
        Task<List<Industry>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(Industry industry, CancellationToken ct = default);
        Task<bool> ExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default);

		Task UpdateAsync(Industry entity, CancellationToken ct = default);
		Task DeleteAsync(Industry entity, CancellationToken ct = default);
	}
}
