using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CMC.Domain.Entities;

namespace CMC.Application.Ports
{
	public interface ILibraryControlRepository
	{
		Task<LibraryControl?> GetByIdAsync(Guid id, CancellationToken ct = default);
		Task<List<LibraryControl>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
		Task<List<LibraryControl>> GetAllAsync(CancellationToken ct = default);

		Task AddAsync(LibraryControl entity, CancellationToken ct = default);
		Task UpdateAsync(LibraryControl entity, CancellationToken ct = default);
		Task DeleteAsync(LibraryControl entity, CancellationToken ct = default);

		// NEU: Query-Methode
		Task<HashSet<Guid>> GetIdsByLibraryScenarioIdsAsync(IEnumerable<Guid> libraryScenarioIds, CancellationToken ct = default);
	}
}
