using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CMC.Domain.Entities;

namespace CMC.Application.Ports
{
	public interface ILibraryScenarioRepository
	{
		Task<LibraryScenario?> GetByIdAsync(Guid id, CancellationToken ct = default);
		Task<List<LibraryScenario>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
		Task<List<LibraryScenario>> GetAllAsync(CancellationToken ct = default);

		Task AddAsync(LibraryScenario entity, CancellationToken ct = default);
		Task UpdateAsync(LibraryScenario entity, CancellationToken ct = default);
		Task DeleteAsync(LibraryScenario entity, CancellationToken ct = default);
	}
}
