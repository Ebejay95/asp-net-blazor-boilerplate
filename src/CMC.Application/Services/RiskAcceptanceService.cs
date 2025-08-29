using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Contracts.RiskAcceptances;
using CMC.Domain.Entities;

namespace CMC.Application.Services
{
	public class RiskAcceptanceService
	{
		private readonly IRiskAcceptanceRepository _repo;

		public RiskAcceptanceService(IRiskAcceptanceRepository repo)
		{
			_repo = repo ?? throw new ArgumentNullException(nameof(repo));
		}

		public async Task<RiskAcceptanceDto> CreateAsync(CreateRiskAcceptanceRequest r, CancellationToken ct = default)
		{
			var ra = new RiskAcceptance(r.CustomerId, r.ControlId, r.Reason, r.RiskAcceptedBy, r.ExpiresAt);
			await _repo.AddAsync(ra, ct);
			return Map(ra);
		}

		public async Task<RiskAcceptanceDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
		{
			var ra = await _repo.GetByIdAsync(id, ct);
			return ra != null ? Map(ra) : null;
		}

		public async Task<List<RiskAcceptanceDto>> GetAllAsync(CancellationToken ct = default)
		{
			var list = await _repo.GetAllAsync(ct);
			return list.Select(Map).ToList();
		}

		public async Task<List<RiskAcceptanceDto>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
		{
			var list = await _repo.GetByCustomerAsync(customerId, ct);
			return list.Select(Map).ToList();
		}

		public async Task<List<RiskAcceptanceDto>> GetActiveByControlAsync(Guid controlId, DateTimeOffset asOfUtc, CancellationToken ct = default)
		{
			var list = await _repo.GetActiveByControlAsync(controlId, asOfUtc, ct);
			return list.Select(Map).ToList();
		}

		public async Task<RiskAcceptanceDto?> UpdateAsync(UpdateRiskAcceptanceRequest r, CancellationToken ct = default)
		{
			var ra = await _repo.GetByIdAsync(r.Id, ct);
			if (ra == null) return null;

			ra.UpdateReason(r.Reason);
			ra.UpdateRiskAcceptedBy(r.RiskAcceptedBy);
			ra.SetExpiry(r.ExpiresAt);
			ra.SetRefs(r.CustomerId, r.ControlId);

			await _repo.UpdateAsync(ra, ct);
			return Map(ra);
		}

		public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
		{
			var ra = await _repo.GetByIdAsync(id, ct);
			if (ra == null) return false;
			await _repo.DeleteAsync(ra, ct);
			return true;
		}

		private static RiskAcceptanceDto Map(RiskAcceptance e) => new RiskAcceptanceDto
		{
			Id = e.Id,
			CustomerId = e.CustomerId,
			ControlId = e.ControlId,
			Reason = e.Reason,
			RiskAcceptedBy = e.RiskAcceptedBy,
			ExpiresAt = e.ExpiresAt.UtcDateTime,
			CreatedAt = e.CreatedAt.UtcDateTime,
			UpdatedAt = e.UpdatedAt.UtcDateTime
		};
	}
}
