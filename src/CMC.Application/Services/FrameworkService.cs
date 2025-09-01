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
		private readonly IFrameworkRepository _frameworkRepo;
		private readonly IIndustryRepository _industryRepo;

		public FrameworkService(IFrameworkRepository frameworkRepo, IIndustryRepository industryRepo)
		{
			_frameworkRepo = frameworkRepo ?? throw new ArgumentNullException(nameof(frameworkRepo));
			_industryRepo  = industryRepo  ?? throw new ArgumentNullException(nameof(industryRepo));
		}

		public async Task<FrameworkDto> CreateAsync(CreateFrameworkRequest request, CancellationToken ct = default)
		{
			if (request == null) throw new ArgumentNullException(nameof(request));
			if (await _frameworkRepo.ExistsAsync(request.Name, request.Version, null, ct))
				throw new DomainException("Framework with this name and version already exists");

			var entity = new Framework(name: request.Name, version: request.Version);

			// Einheitlich: IDs validieren (batch) und Domain-Setter nutzen
			var ids = (request.IndustryIds ?? Array.Empty<Guid>())
				.Where(x => x != Guid.Empty).Distinct().ToArray();

			if (ids.Length > 0)
			{
				var industries = await _industryRepo.GetByIdsAsync(ids, ct);
				if (industries.Count != ids.Length)
					throw new DomainException("One or more specified industries do not exist");
				entity.SetIndustries(ids);
			}

			await _frameworkRepo.AddAsync(entity, ct);
			return MapToDto(entity);
		}

		public async Task<FrameworkDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
		{
			var entity = await _frameworkRepo.GetByIdAsync(id, ct);
			return entity != null ? MapToDto(entity) : null;
		}

		public async Task<List<FrameworkDto>> GetAllAsync(CancellationToken ct = default)
		{
			var items = await _frameworkRepo.GetAllAsync(ct);
			return items.Select(MapToDto).ToList();
		}

		public async Task<List<FrameworkDto>> GetByIndustryAsync(Guid industryId, CancellationToken ct = default)
		{
			var items = await _frameworkRepo.GetByIndustryIdAsync(industryId, ct);
			return items.Select(MapToDto).ToList();
		}

		public async Task<FrameworkDto?> UpdateAsync(UpdateFrameworkRequest request, CancellationToken ct = default)
		{
			if (request == null) throw new ArgumentNullException(nameof(request));

			var entity = await _frameworkRepo.GetByIdAsync(request.Id, ct);
			if (entity == null) return null;

			if (await _frameworkRepo.ExistsAsync(request.Name, request.Version, request.Id, ct))
				throw new DomainException("Framework with this name and version already exists");

			entity.Rename(request.Name);
			entity.SetVersion(request.Version);

			if (request.IndustryIds != null)
			{
				var ids = request.IndustryIds.Where(x => x != Guid.Empty).Distinct().ToArray();
				var industries = await _industryRepo.GetByIdsAsync(ids, ct);
				if (industries.Count != ids.Length)
					throw new DomainException("One or more specified industries do not exist");
				entity.SetIndustries(ids);
			}

			await _frameworkRepo.UpdateAsync(entity, ct);

			var updated = await _frameworkRepo.GetByIdAsync(entity.Id, ct);
			return updated != null ? MapToDto(updated) : null;
		}

		public async Task<bool> DeleteAsync(DeleteFrameworkRequest request, CancellationToken ct = default)
		{
			if (request is null) throw new ArgumentNullException(nameof(request));
			return await DeleteAsync(request.Id, ct);
		}

		public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
		{
			var entity = await _frameworkRepo.GetByIdAsync(id, ct);
			if (entity == null) return false;
			await _frameworkRepo.DeleteAsync(entity, ct);
			return true;
		}

		// -------------------- Helpers --------------------
		private static FrameworkDto MapToDto(Framework e)
		{
			var industryIds = e.IndustryLinks?
				.Select(l => l.IndustryId)
				.Distinct()
				.ToArray() ?? Array.Empty<Guid>();

			var industryNames = e.IndustryLinks?
				.Select(l => l.Industry?.Name)
				.Where(n => !string.IsNullOrWhiteSpace(n))
				.Distinct()
				.ToArray() ?? Array.Empty<string>();

			return new FrameworkDto(
				e.Id,
				e.Name,
				e.Version,
				industryIds,
				industryNames,
				e.CreatedAt,
				e.UpdatedAt
			);
		}
	}
}
