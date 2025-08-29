using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Contracts.Controls;
using CMC.Domain.Entities;

namespace CMC.Application.Services
{
	public class ControlService
	{
		private readonly IControlRepository _repo;
		private readonly ILibraryControlRepository _libRepo;
		private readonly IEvidenceRepository _evidenceRepo;

		public ControlService(IControlRepository repo, ILibraryControlRepository libRepo, IEvidenceRepository evidenceRepo)
		{
			_repo = repo ?? throw new ArgumentNullException(nameof(repo));
			_libRepo = libRepo ?? throw new ArgumentNullException(nameof(libRepo));
			_evidenceRepo = evidenceRepo ?? throw new ArgumentNullException(nameof(evidenceRepo));
		}

		// CREATE
		public async Task<ControlDto> CreateAsync(CreateControlRequest r, CancellationToken ct = default)
		{
			var control = new Control(
				customerId: r.CustomerId,
				libraryControlId: r.LibraryControlId,
				implemented: r.Implemented,
				coverage: r.Coverage,
				maturity: r.Maturity,
				evidenceWeight: r.EvidenceWeight,
				evidenceId: r.EvidenceId,
				freshness: r.Freshness,
				costTotalEur: r.CostTotalEur,
				deltaEalEur: r.DeltaEalEur,
				score: r.Score,
				status: r.Status,
				dueDateUtc: r.DueDate
			);
			await _repo.AddAsync(control, ct);
			return Map(control);
		}

		// READ
		public async Task<ControlDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
		{
			var c = await _repo.GetByIdAsync(id, ct);
			return c != null ? Map(c) : null;
		}

		public async Task<List<ControlDto>> GetAllAsync(CancellationToken ct = default)
		{
			var items = await _repo.GetAllAsync(ct);
			return items.Select(Map).ToList();
		}

		public async Task<List<ControlDto>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
		{
			var items = await _repo.GetByCustomerAsync(customerId, ct);
			return items.Select(Map).ToList();
		}

		// UPDATE
		public async Task<ControlDto?> UpdateAsync(UpdateControlRequest r, CancellationToken ct = default)
		{
			var c = await _repo.GetByIdAsync(r.Id, ct);
			if (c == null) return null;

			c.SetImplemented(r.Implemented);
			c.SetCoverage(r.Coverage);
			c.SetMaturity(r.Maturity);
			c.SetEvidenceWeight(r.EvidenceWeight);
			c.SetFreshness(r.Freshness);
			c.SetCosts(r.CostTotalEur);
			c.SetDeltaEal(r.DeltaEalEur);
			c.SetScore(r.Score);
			c.SetStatus(r.Status);
			c.SetDueDate(r.DueDate);
			c.LinkEvidence(r.EvidenceId);

			await _repo.UpdateAsync(c, ct);
			return Map(c);
		}

		// DELETE
		public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
		{
			var c = await _repo.GetByIdAsync(id, ct);
			if (c == null) return false;
			await _repo.DeleteAsync(c, ct);
			return true;
		}

		// BUSINESS (no-brainer)
		public async Task<ControlDto?> TransitionAsync(ChangeControlStatusRequest r, CancellationToken ct = default)
		{
			var c = await _repo.GetByIdAsync(r.ControlId, ct);
			if (c == null) return null;

			c.TransitionTo(r.NewStatus, r.AsOfUtc);
			await _repo.UpdateAsync(c, ct);
			return Map(c);
		}

		public async Task<ControlDto?> LinkEvidenceAsync(LinkEvidenceRequest r, CancellationToken ct = default)
		{
			var c = await _repo.GetByIdAsync(r.ControlId, ct);
			if (c == null) return null;

			if (r.EvidenceId.HasValue)
			{
				var ev = await _evidenceRepo.GetByIdAsync(r.EvidenceId.Value, ct);
				if (ev == null) throw new InvalidOperationException("Evidence not found");
			}

			c.LinkEvidence(r.EvidenceId);
			await _repo.UpdateAsync(c, ct);
			return Map(c);
		}

		// Provisionierung aus LibraryControls (einfacher Startwert)
		public async Task<List<ControlDto>> ProvisionFromLibraryAsync(Guid customerId, IEnumerable<Guid> libraryControlIds, CancellationToken ct = default)
		{
			var libs = await _libRepo.GetByIdsAsync(libraryControlIds, ct);
			var list = libs.Select(lib => Control.FromLibrary(
				customerId: customerId,
				lib: lib,
				implemented: false,
				coverage: 0m,
				maturity: 0,
				evidenceWeight: 0m,
				freshness: 0m,
				costTotalEur: lib.OpexYearEur + lib.CapexEur,
				deltaEalEur: 0m,
				score: 0m,
				status: "proposed"
			)).ToList();

			foreach (var c in list) await _repo.AddAsync(c, ct);
			return list.Select(Map).ToList();
		}

		private static ControlDto Map(Control c) => new ControlDto
		{
			Id = c.Id,
			CustomerId = c.CustomerId,
			LibraryControlId = c.LibraryControlId,
			EvidenceId = c.EvidenceId,
			Implemented = c.Implemented,
			Coverage = c.Coverage,
			Maturity = c.Maturity,
			EvidenceWeight = c.EvidenceWeight,
			Freshness = c.Freshness,
			CostTotalEur = c.CostTotalEur,
			DeltaEalEur = c.DeltaEalEur,
			Score = c.Score,
			Status = c.Status,
			DueDate = c.DueDate,
			CreatedAt = c.CreatedAt.UtcDateTime,
			UpdatedAt = c.UpdatedAt.UtcDateTime
		};
	}
}
