using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Contracts.Frameworks;
using CMC.Domain.Common;
using CMC.Domain.Entities;

namespace CMC.Application.Services
{
	public class FrameworkService
	{
		private readonly IFrameworkRepository _repository;
		private readonly IIndustryRepository _industryRepository;

		public FrameworkService(IFrameworkRepository repository, IIndustryRepository industryRepository)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_industryRepository = industryRepository ?? throw new ArgumentNullException(nameof(industryRepository));
		}

		public async Task<FrameworkDto> CreateAsync(CreateFrameworkRequest request, CancellationToken cancellationToken = default)
		{
			if (request == null) throw new ArgumentNullException(nameof(request));
			if (await _repository.ExistsAsync(request.Name, request.Version, null, cancellationToken))
				throw new DomainException("Framework with this name and version already exists");

			var entity = new Framework(name: request.Name, version: request.Version);

			// Industries: upsert + links
			var tokens = ParseIndustryTokens(request.Industry);
			if (tokens.Length > 0)
			{
				var industries = await EnsureIndustriesAsync(tokens, cancellationToken);
				SyncFrameworkIndustries(entity, industries);
			}

			await _repository.AddAsync(entity, cancellationToken);
			return MapToDto(entity);
		}

		public async Task<FrameworkDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			// WICHTIG: Repo sollte IndustryLinks inkl. Industry mitladen!
			var entity = await _repository.GetByIdAsync(id, cancellationToken);
			return entity != null ? MapToDto(entity) : null;
		}

		public async Task<List<FrameworkDto>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			// WICHTIG: Repo sollte IndustryLinks inkl. Industry mitladen!
			var items = await _repository.GetAllAsync(cancellationToken);
			return items.Select(MapToDto).ToList();
		}

		public async Task<List<FrameworkDto>> GetByIndustryAsync(string industry, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(industry)) throw new ArgumentException("Industry cannot be null or empty", nameof(industry));
			var items = await _repository.GetByIndustryAsync(industry, cancellationToken);
			return items.Select(MapToDto).ToList();
		}

		public async Task<FrameworkDto?> UpdateAsync(UpdateFrameworkRequest request, CancellationToken cancellationToken = default)
		{
			if (request == null) throw new ArgumentNullException(nameof(request));

			// WICHTIG: Repo sollte IndustryLinks inkl. Industry mitladen!
			var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
			if (entity == null) return null;

			if (await _repository.ExistsAsync(request.Name, request.Version, request.Id, cancellationToken))
				throw new DomainException("Framework with this name and version already exists");

			entity.Rename(request.Name);
			entity.SetVersion(request.Version);

			// Industries: upsert + links sync (optional, falls im Request gesetzt)
			if (request.Industry != null)
			{
				var tokens = ParseIndustryTokens(request.Industry);
				var industries = await EnsureIndustriesAsync(tokens, cancellationToken);
				SyncFrameworkIndustries(entity, industries);
			}

			await _repository.UpdateAsync(entity, cancellationToken);

			var updated = await _repository.GetByIdAsync(entity.Id, cancellationToken);
			return updated != null ? MapToDto(updated) : null;
		}

		public async Task<bool> DeleteAsync(DeleteFrameworkRequest request, CancellationToken cancellationToken = default)
		{
			if (request is null) throw new ArgumentNullException(nameof(request));
			return await DeleteAsync(request.Id, cancellationToken);
		}

		public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
		{
			var entity = await _repository.GetByIdAsync(id, cancellationToken);
			if (entity == null) return false;
			await _repository.DeleteAsync(entity, cancellationToken);
			return true;
		}

		// -------------------- Helpers --------------------

		private static string BuildIndustryString(Framework e)
		{
			var names = e.IndustryLinks?
				.Select(l => l.Industry?.Name)
				.Where(n => !string.IsNullOrWhiteSpace(n))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.OrderBy(n => n)
				.ToArray();

			return (names == null || names.Length == 0) ? string.Empty : string.Join(", ", names);
		}

		private static FrameworkDto MapToDto(Framework e) => new(
			Id: e.Id,
			Name: e.Name,
			Version: e.Version,
			Industry: BuildIndustryString(e),
			CreatedAt: e.CreatedAt.UtcDateTime,
			UpdatedAt: e.UpdatedAt.UtcDateTime
		);

		private static string[] ParseIndustryTokens(string? raw)
		{
			if (string.IsNullOrWhiteSpace(raw)) return Array.Empty<string>();
			char[] seps = new[] { ',', ';', '|', '/' };
			return raw
				.Split(seps, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.Select(s => s.Trim())
				.Where(s => s.Length > 0)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToArray();
		}

		private async Task<List<Industry>> EnsureIndustriesAsync(string[] tokens, CancellationToken ct)
		{
			var result = new List<Industry>(tokens.Length);
			foreach (var name in tokens)
			{
				var existing = await _industryRepository.GetByNameAsync(name, ct);
				if (existing == null)
				{
					existing = new Industry(name);
					await _industryRepository.AddAsync(existing, ct);
				}
				result.Add(existing);
			}
			return result;
		}

		private static void SyncFrameworkIndustries(Framework f, IReadOnlyCollection<Industry> target)
		{
			var targetIds = target.Select(i => i.Id).ToHashSet();
			var toRemove = f.IndustryLinks.Where(l => !targetIds.Contains(l.IndustryId)).ToList();
			foreach (var r in toRemove) f.IndustryLinks.Remove(r);

			var existingIds = f.IndustryLinks.Select(l => l.IndustryId).ToHashSet();
			foreach (var ind in target)
			{
				if (!existingIds.Contains(ind.Id))
					f.IndustryLinks.Add(new FrameworkIndustry(f.Id, ind.Id));
			}
		}
	}
}
