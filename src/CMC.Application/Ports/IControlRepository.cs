using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CMC.Domain.Entities;

namespace CMC.Application.Ports
{
	public interface IControlRepository
	{
		Task<Control?> GetByIdAsync(Guid id, CancellationToken ct = default);
		Task<List<Control>> GetAllAsync(CancellationToken ct = default);
		Task<List<Control>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);
		Task AddAsync(Control control, CancellationToken ct = default);
		Task UpdateAsync(Control control, CancellationToken ct = default);
		Task DeleteAsync(Control control, CancellationToken ct = default);
		Task AddRangeAsync(IEnumerable<Control> controls, CancellationToken ct = default);
	}
}
