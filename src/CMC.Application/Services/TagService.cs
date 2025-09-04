using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Contracts.Tags;
using CMC.Domain.Common;
using CMC.Domain.Entities;

namespace CMC.Application.Services
{
	public class TagService
	{
		private readonly ITagRepository _repo;

		public TagService(ITagRepository repo)
		{
			_repo = repo ?? throw new ArgumentNullException(nameof(repo));
		}

		public async Task<TagDto> CreateAsync(CreateTagRequest r, CancellationToken ct = default)
		{
			var existing = await _repo.GetByNameAsync(r.Name, ct);
			if (existing != null) throw new ArgumentException("Tag already exists", nameof(r.Name));

			var e = new Tag(r.Name);
			await _repo.AddAsync(e, ct);
			return Map(e);
		}

		public async Task<TagDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
		{
			var e = await _repo.GetByIdAsync(id, ct);
			return e != null ? Map(e) : null;
		}

		public async Task<List<TagDto>> GetAllAsync(CancellationToken ct = default)
		{
			var list = await _repo.GetAllAsync(ct);
			return list.Select(Map).ToList();
		}

		public async Task<TagDto?> UpdateAsync(UpdateTagRequest r, CancellationToken ct = default)
		{
			var e = await _repo.GetByIdAsync(r.Id, ct);
			if (e == null) return null;

			if (await _repo.ExistsAsync(r.Name, r.Id, ct))
				throw new DomainException("Tag already exists");

			e.Rename(r.Name);
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

		private static TagDto Map(Tag e) => new TagDto
		{
			Id = e.Id,
			Name = e.Name
		};
	}
}
