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

        public async Task<ControlDto> CreateAsync(CreateControlRequest r, CancellationToken ct = default)
        {
            var c = new Control(
                customerId:      r.CustomerId,
                libraryControlId:r.LibraryControlId,
                implemented:     r.Implemented,
                coverage:        r.Coverage,
                maturity:        r.Maturity,
                evidenceWeight:  r.EvidenceWeight,
                evidenceId:      r.EvidenceId,
                freshness:       r.Freshness,
                costTotalEur:    r.CostTotalEur
            );

            if (!string.IsNullOrWhiteSpace(r.InitialStatusTag))
                c.TransitionTo(r.InitialStatusTag!, DateTimeOffset.UtcNow);

            if (r.DueDate.HasValue) c.SetDueDate(r.DueDate);

            // NEU: Relationen setzen
            c.SetTags(r.TagIds ?? Array.Empty<Guid>());
            c.SetIndustries(r.IndustryIds ?? Array.Empty<Guid>());

            await _repo.AddAsync(c, ct);
            return Map(c);
        }

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
            if (r.DueDate.HasValue) c.SetDueDate(r.DueDate);
            c.LinkEvidence(r.EvidenceId);

            if (!string.IsNullOrWhiteSpace(r.StatusTag))
                c.TransitionTo(r.StatusTag!, DateTimeOffset.UtcNow);

            // NEU: Relationen
            c.SetTags(r.TagIds ?? Array.Empty<Guid>());
            c.SetIndustries(r.IndustryIds ?? Array.Empty<Guid>());

            await _repo.UpdateAsync(c, ct);
            return Map(c);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var c = await _repo.GetByIdAsync(id, ct);
            if (c == null) return false;
            await _repo.DeleteAsync(c, ct);
            return true;
        }

        public async Task<ControlDto?> TransitionAsync(ChangeControlStatusRequest r, CancellationToken ct = default)
        {
            var c = await _repo.GetByIdAsync(r.ControlId, ct);
            if (c == null) return null;

            var asOf = r.AsOfUtc ?? DateTimeOffset.UtcNow;
            c.TransitionTo(r.NewStatus, asOf);

            await _repo.UpdateAsync(c, ct);
            return Map(c);
        }

        public async Task<List<ControlDto>> ProvisionFromLibraryAsync(
            Guid customerId,
            IEnumerable<Guid> libraryControlIds,
            CancellationToken ct = default)
        {
            var libs = await _libRepo.GetByIdsAsync(libraryControlIds, ct);

            var list = new List<Control>();
            foreach (var lib in libs)
            {
                var ctrl = Control.FromLibrary(
                    customerId: customerId,
                    lib: lib,
                    implemented: false,
                    coverage: 0m,
                    maturity: 0,
                    evidenceWeight: 0m,
                    freshness: 0m,
                    costTotalEur: lib.OpexYearEur + lib.CapexEur
                );
                ctrl.TransitionTo("proposed", DateTimeOffset.UtcNow);
                list.Add(ctrl);
            }

            await _repo.AddRangeAsync(list, ct);
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
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,

            // Initialwerte optional – Editor lädt Ist-Zuordnung ohnehin über RelationshipManager.
            TagIds = Array.Empty<Guid>(),
            IndustryIds = Array.Empty<Guid>(),
            TagLabels = Array.Empty<string>(),
            IndustryLabels = Array.Empty<string>()
        };
    }
}
