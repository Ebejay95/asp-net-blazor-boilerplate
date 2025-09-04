using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Contracts.LibraryControls;
using CMC.Domain.Entities;

namespace CMC.Application.Services
{
    public class LibraryControlService
    {
        private readonly ILibraryControlRepository _repo;

        public LibraryControlService(ILibraryControlRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<LibraryControlDto> CreateAsync(CreateLibraryControlRequest r, CancellationToken ct = default)
        {
            var e = new LibraryControl(
                name: r.Name,
                capexEur: r.CapexEur,
                opexYearEur: r.OpexYearEur,
                internalDays: r.InternalDays,
                externalDays: r.ExternalDays,
                tagIds: r.TagIds,
                industryIds: r.IndustryIds,
                totalDays: r.TotalDays
            );

            e.SetLibraryScenarios(r.LibraryScenarioIds);
            await _repo.AddAsync(e, ct);
            return Map(e);
        }

        public async Task<LibraryControlDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var e = await _repo.GetByIdAsync(id, ct);
            return e != null ? Map(e) : null;
        }

        public async Task<List<LibraryControlDto>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await _repo.GetAllAsync(ct);
            return list.Select(Map).ToList();
        }

        public async Task<LibraryControlDto?> UpdateAsync(UpdateLibraryControlRequest r, CancellationToken ct = default)
        {
            var e = await _repo.GetByIdAsync(r.Id, ct);
            if (e == null) return null;

            e.UpdateEffort(r.InternalDays, r.ExternalDays, r.TotalDays);
            e.UpdateCosts(r.CapexEur, r.OpexYearEur);
            e.SetTags(r.TagIds);
            e.SetIndustries(r.IndustryIds);
            e.SetLibraryScenarios(r.LibraryScenarioIds);

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

        // Direkte Repository-Delegation
        public Task<HashSet<Guid>> GetIdsByLibraryScenarioIdsAsync(IEnumerable<Guid> libraryScenarioIds, CancellationToken ct = default)
            => _repo.GetIdsByLibraryScenarioIdsAsync(libraryScenarioIds, ct);

        private static LibraryControlDto Map(LibraryControl e) => new LibraryControlDto
        {
            Id = e.Id,
            Name = e.Name,
            CapexEur = e.CapexEur,
            OpexYearEur = e.OpexYearEur,
            InternalDays = e.InternalDays,
            ExternalDays = e.ExternalDays,
            TotalDays = e.TotalDays,

            TagIds = e.GetTagIds(),
            IndustryIds = e.GetIndustryIds(),
            LibraryScenarioIds = e.GetLibraryScenarioIds(),

            TagLabels = e.TagLinks.Select(l => l.Tag?.Name).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToArray(),
            IndustryLabels = e.IndustryLinks.Select(l => l.Industry?.Name).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToArray(),
            LibraryScenarioLabels = e.ScenarioLinks.Select(l => l.LibraryScenario?.Name).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToArray(),

            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };
    }
}
