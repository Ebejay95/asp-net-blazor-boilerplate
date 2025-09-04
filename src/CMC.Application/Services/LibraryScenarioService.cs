using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Contracts.LibraryScenarios;
using CMC.Domain.Entities;

namespace CMC.Application.Services
{
    public class LibraryScenarioService
    {
        private readonly ILibraryScenarioRepository _repo;

        public LibraryScenarioService(ILibraryScenarioRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<LibraryScenarioDto> CreateAsync(CreateLibraryScenarioRequest r, CancellationToken ct = default)
        {
            var e = new LibraryScenario(
                name: r.Name,
                annualFrequency: r.AnnualFrequency,
                impactPctRevenue: r.ImpactPctRevenue,
                tagIds: r.TagIds ?? Array.Empty<Guid>(),
                industryIds: r.IndustryIds ?? Array.Empty<Guid>()
            );
            await _repo.AddAsync(e, ct);
            return Map(e);
        }

        public async Task<LibraryScenarioDto?> UpdateAsync(UpdateLibraryScenarioRequest r, CancellationToken ct = default)
        {
            var e = await _repo.GetByIdAsync(r.Id, ct);
            if (e == null) return null;

            e.Rename(r.Name);
            e.SetAnnualFrequency(r.AnnualFrequency);
            e.SetImpactPctRevenue(r.ImpactPctRevenue);
            e.SetTags(r.TagIds ?? Array.Empty<Guid>());
            e.SetIndustries(r.IndustryIds ?? Array.Empty<Guid>());

            await _repo.UpdateAsync(e, ct);
            return Map(e);
        }

        public async Task<LibraryScenarioDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var e = await _repo.GetByIdAsync(id, ct);
            return e != null ? Map(e) : null;
        }

        public async Task<List<LibraryScenarioDto>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await _repo.GetAllAsync(ct);
            return list.Select(Map).ToList();
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var e = await _repo.GetByIdAsync(id, ct);
            if (e == null) return false;
            await _repo.DeleteAsync(e, ct);
            return true;
        }

        // Direkte Repository-Delegation
        public Task<List<LibraryScenarioLite>> GetByIndustriesAsync(IEnumerable<Guid> industryIds, CancellationToken ct = default)
            => _repo.GetByIndustriesAsync(industryIds, ct);

        private static LibraryScenarioDto Map(LibraryScenario e) => new LibraryScenarioDto
        {
            Id = e.Id,
            Name = e.Name,
            AnnualFrequency = e.AnnualFrequency,
            ImpactPctRevenue = e.ImpactPctRevenue,

            TagIds = e.GetTagIds(),
            IndustryIds = e.GetIndustryIds(),

            TagLabels = e.TagLinks?
                .Select(l => l.Tag?.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Cast<string>()
                .Distinct()
                .ToArray() ?? Array.Empty<string>(),

            IndustryLabels = e.IndustryLinks?
                .Select(l => l.Industry?.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Cast<string>()
                .Distinct()
                .ToArray() ?? Array.Empty<string>(),

            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };
    }
}
