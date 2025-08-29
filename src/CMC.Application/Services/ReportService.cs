using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Contracts.Reports;
using CMC.Domain.Entities;

namespace CMC.Application.Services
{
	public class ReportService
	{
		private readonly IReportRepository _repo;

		public ReportService(IReportRepository repo)
		{
			_repo = repo ?? throw new ArgumentNullException(nameof(repo));
		}

		public async Task<ReportDto> CreateAsync(CreateReportRequest r, CancellationToken ct = default)
		{
			var e = new Report(r.DefinitionId, r.PeriodStart, r.PeriodEnd, r.GeneratedAt, r.Frozen, r.CustomerId);
			await _repo.AddAsync(e, ct);
			return Map(e);
		}

		public async Task<ReportDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
		{
			var e = await _repo.GetByIdAsync(id, ct);
			return e != null ? Map(e) : null;
		}

		public async Task<List<ReportDto>> GetAllAsync(CancellationToken ct = default)
		{
			var list = await _repo.GetAllAsync(ct);
			return list.Select(Map).ToList();
		}

		public async Task<List<ReportDto>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
		{
			var list = await _repo.GetByCustomerAsync(customerId, ct);
			return list.Select(Map).ToList();
		}

		public async Task<ReportDto?> UpdateAsync(UpdateReportRequest r, CancellationToken ct = default)
		{
			var e = await _repo.GetByIdAsync(r.Id, ct);
			if (e == null) return null;

			e.SetDefinition(r.DefinitionId);
			e.SetPeriod(r.PeriodStart, r.PeriodEnd);
			if (r.Frozen == true) e.Freeze(); else if (r.Frozen == false) e.Unfreeze();
			e.Regenerate(r.GeneratedAt);
			e.ReassignCustomer(r.CustomerId);

			await _repo.UpdateAsync(e, ct);
			return Map(e);
		}

		public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
		{
			var e = await _repo.GetByIdAsync(id, ct);
			if (e == null) return false;
			await _repo.DeleteAsync(e, ct);
			return true;
		}

		private static ReportDto Map(Report e) => new ReportDto
		{
			Id = e.Id,
			CustomerId = e.CustomerId,
			DefinitionId = e.DefinitionId,
			PeriodStart = e.PeriodStart.UtcDateTime,
			PeriodEnd = e.PeriodEnd.UtcDateTime,
			GeneratedAt = e.GeneratedAt.UtcDateTime,
			Frozen = e.Frozen,
			CreatedAt = e.CreatedAt.UtcDateTime,
			UpdatedAt = e.UpdatedAt.UtcDateTime
		};
	}
}
