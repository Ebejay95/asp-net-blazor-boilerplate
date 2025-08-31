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
	public class ReportDefinitionService
	{
		private readonly IReportDefinitionRepository _repo;

		public ReportDefinitionService(IReportDefinitionRepository repo)
		{
			_repo = repo ?? throw new ArgumentNullException(nameof(repo));
		}

		public async Task<ReportDefinitionDto> CreateAsync(CreateReportDefinitionRequest r, CancellationToken ct = default)
		{
			var e = new ReportDefinition(r.CustomerId, r.Name, r.Kind, r.WindowDays, r.Sections);
			await _repo.AddAsync(e, ct);
			return Map(e);
		}

		public async Task<ReportDefinitionDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
		{
			var e = await _repo.GetByIdAsync(id, ct);
			return e != null ? Map(e) : null;
		}

		public async Task<List<ReportDefinitionDto>> GetAllAsync(CancellationToken ct = default)
		{
			var list = await _repo.GetAllAsync(ct);
			return list.Select(Map).ToList();
		}

		public async Task<List<ReportDefinitionDto>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
		{
			var list = await _repo.GetByCustomerAsync(customerId, ct);
			return list.Select(Map).ToList();
		}

		public async Task<ReportDefinitionDto?> UpdateAsync(UpdateReportDefinitionRequest r, CancellationToken ct = default)
		{
			var e = await _repo.GetByIdAsync(r.Id, ct);
			if (e == null) return null;

			e.Rename(r.Name);
			e.SetKind(r.Kind);
			e.SetWindowDays(r.WindowDays);
			e.SetSections(r.Sections);

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

		public async Task<(DateTimeOffset Start, DateTimeOffset End)?> CalculatePeriodAsync(Guid definitionId, DateTimeOffset referenceUtc, CancellationToken ct = default)
		{
			var e = await _repo.GetByIdAsync(definitionId, ct);
			if (e == null) return null;
			return e.CalculatePeriod(referenceUtc);
		}

		private static ReportDefinitionDto Map(ReportDefinition e) => new ReportDefinitionDto
		{
			Id = e.Id,
			CustomerId = e.CustomerId,
			Name = e.Name,
			Kind = e.Kind,
			WindowDays = e.WindowDays,
			Sections = e.Sections,
			CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
		};
	}
}
