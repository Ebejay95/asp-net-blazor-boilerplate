using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Contracts.Scenarios;
using CMC.Domain.Entities;

namespace CMC.Application.Services
{
    public class ScenarioService
    {
        private readonly IScenarioRepository _repo;
        private readonly ILibraryScenarioRepository _libRepo;

        public ScenarioService(
            IScenarioRepository repo,
            ILibraryScenarioRepository libRepo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _libRepo = libRepo ?? throw new ArgumentNullException(nameof(libRepo));
        }

        public async Task<ScenarioDto> CreateAsync(CreateScenarioRequest r, CancellationToken ct = default)
        {
            var s = new Scenario(r.CustomerId, r.LibraryScenarioId, r.Name, r.AnnualFrequency, r.ImpactPctRevenue, r.TagIds);
            await _repo.AddAsync(s, ct);
            var reloaded = await _repo.GetByIdAsync(s.Id, ct) ?? s;
            return Map(reloaded);
        }

        public async Task<ScenarioDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var s = await _repo.GetByIdAsync(id, ct);
            return s != null ? Map(s) : null;
        }

        public async Task<List<ScenarioDto>> GetAllAsync(CancellationToken ct = default)
        {
            var items = await _repo.GetAllAsync(ct);
            return items.Select(Map).ToList();
        }

        public async Task<List<ScenarioDto>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
        {
            var items = await _repo.GetByCustomerAsync(customerId, ct);
            return items.Select(Map).ToList();
        }

        public async Task<ScenarioDto?> UpdateAsync(UpdateScenarioRequest r, CancellationToken ct = default)
        {
            var s = await _repo.GetByIdAsync(r.Id, ct);
            if (s == null) return null;

            s.Rename(r.Name);
            s.SetAnnualFrequency(r.AnnualFrequency);
            s.SetImpactPctRevenue(r.ImpactPctRevenue);
            s.SetTags(r.TagIds);

            await _repo.UpdateAsync(s, ct);

            var reloaded = await _repo.GetByIdAsync(s.Id, ct) ?? s;
            return Map(reloaded);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var s = await _repo.GetByIdAsync(id, ct);
            if (s == null) return false;
            await _repo.DeleteAsync(s, ct);
            return true;
        }

        public async Task<List<ScenarioDto>> ProvisionFromLibraryAsync(Guid customerId, IEnumerable<Guid> libraryScenarioIds, CancellationToken ct = default)
        {
            var libs = await _libRepo.GetByIdsAsync(libraryScenarioIds, ct);
            var scenarios = ProvisioningService.MaterializeScenarios(customerId, libs);
            await _repo.AddRangeAsync(scenarios, ct);

            var ids = scenarios.Select(x => x.Id).ToArray();
            var result = new List<ScenarioDto>(scenarios.Count);
            foreach (var id in ids)
            {
                var s = await _repo.GetByIdAsync(id, ct);
                if (s != null) result.Add(Map(s));
            }
            return result;
        }

        // Direkte Repository-Delegation
        public Task<int> CountByCustomerAsync(Guid customerId, CancellationToken ct = default)
            => _repo.CountByCustomerAsync(customerId, ct);

        private static ScenarioDto Map(Scenario s) => new ScenarioDto
        {
            Id = s.Id,
            CustomerId = s.CustomerId,
            CustomerName = s.Customer?.Name ?? string.Empty,
            LibraryScenarioId = s.LibraryScenarioId,
            LibraryScenarioName = s.LibraryScenario?.Name ?? string.Empty,
            Name = s.Name,
            AnnualFrequency = s.AnnualFrequency,
            ImpactPctRevenue = s.ImpactPctRevenue,
            TagIds = s.GetTagIds(),
            TagLabels = s.TagLinks?.Select(t => t.Tag?.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Cast<string>().Distinct().ToArray()
                        ?? Array.Empty<string>(),
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };
    }
}
