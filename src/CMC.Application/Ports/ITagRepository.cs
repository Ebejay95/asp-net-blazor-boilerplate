using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CMC.Domain.Entities;

namespace CMC.Application.Ports
{
	public interface ITagRepository
	{
		Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct = default);
		Task<Tag?> GetByNameAsync(string name, CancellationToken ct = default);
		Task<List<Tag>> GetAllAsync(CancellationToken ct = default);
		Task AddAsync(Tag tag, CancellationToken ct = default);
		Task<bool> ExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
		Task UpdateAsync(Tag entity, CancellationToken ct = default);
		Task DeleteAsync(Tag entity, CancellationToken ct = default);

		Task<List<Tag>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
	}
}
