using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Contracts.Evidences;
using CMC.Domain.Entities;

namespace CMC.Application.Services
{
	public class EvidenceService
	{
		private readonly IEvidenceRepository _repo;

		public EvidenceService(IEvidenceRepository repo)
		{
			_repo = repo ?? throw new ArgumentNullException(nameof(repo));
		}

		public async Task<EvidenceDto> CreateAsync(CreateEvidenceRequest r, CancellationToken ct = default)
		{
			var e = new Evidence(r.CustomerId, r.Source, r.CollectedAt, r.Location, r.ValidUntil, r.HashSha256, r.Confidentiality);
			await _repo.AddAsync(e, ct);
			return Map(e);
		}

		public async Task<EvidenceDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
		{
			var e = await _repo.GetByIdAsync(id, ct);
			return e != null ? Map(e) : null;
		}

		public async Task<List<EvidenceDto>> GetAllAsync(CancellationToken ct = default)
		{
			var list = await _repo.GetAllAsync(ct);
			return list.Select(Map).ToList();
		}

		public async Task<List<EvidenceDto>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
		{
			var list = await _repo.GetByCustomerAsync(customerId, ct);
			return list.Select(Map).ToList();
		}

		public async Task<EvidenceDto?> UpdateAsync(UpdateEvidenceRequest r, CancellationToken ct = default)
		{
			var e = await _repo.GetByIdAsync(r.Id, ct);
			if (e == null) return null;

			e.UpdateSource(r.Source);
			e.SetLocation(r.Location);
			e.SetValidity(r.ValidUntil);
			e.SetHashSha256(r.HashSha256);
			e.SetConfidentiality(r.Confidentiality);

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

		private static EvidenceDto Map(Evidence e) => new EvidenceDto
		{
			Id = e.Id,
			CustomerId = e.CustomerId,
			Source = e.Source,
			Location = e.Location,
			CollectedAt = e.CollectedAt,
			ValidUntil  = e.ValidUntil,
			HashSha256 = e.HashSha256,
			Confidentiality = e.Confidentiality,
            CreatedAt   = e.CreatedAt,
            UpdatedAt   = e.UpdatedAt
		};
	}
}
