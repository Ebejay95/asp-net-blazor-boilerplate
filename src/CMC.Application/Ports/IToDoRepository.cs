using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CMC.Domain.Entities;

namespace CMC.Application.Ports
{
	public interface IToDoRepository
	{
		Task<ToDo?> GetByIdAsync(Guid id, CancellationToken ct = default);
		Task<List<ToDo>> GetAllAsync(CancellationToken ct = default);
		Task<List<ToDo>> GetByControlIdAsync(Guid controlId, CancellationToken ct = default);
		Task AddAsync(ToDo task, CancellationToken ct = default);
		Task AddRangeAsync(IEnumerable<ToDo> tasks, CancellationToken ct = default);
		Task UpdateAsync(ToDo task, CancellationToken ct = default);
		Task DeleteAsync(ToDo task, CancellationToken ct = default);
	}
}
