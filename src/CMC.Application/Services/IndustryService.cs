using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Contracts.Industries;
using CMC.Domain.Entities;

namespace CMC.Application.Services
{
	public class IndustryService
	{
		private readonly IIndustryRepository _repo;

		public IndustryService(IIndustryRepository repo)
		{
			_repo = repo ?? throw new ArgumentNullException(nameof(repo));
		}

		public async Task<IndustryDto> CreateAsync(CreateIndustryRequest r, CancellationToken ct = default)
		{
			var existing = await _repo.GetByNameAsync(r.Name, ct);
			if (existing != null) throw new ArgumentException("Industry already exists", nameof(r.Name));

			var e = new Industry(r.Name);
			await _repo.AddAsync(e, ct);
			return Map(e);
		}

		public async Task<IndustryDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
		{
			var e = await _repo.GetByIdAsync(id, ct);
			return e != null ? Map(e) : null;
		}

		public async Task<List<IndustryDto>> GetAllAsync(CancellationToken ct = default)
		{
			var list = await _repo.GetAllAsync(ct);
			return list.Select(Map).ToList();
		}

        public async Task<IndustryDto?> UpdateAsync(UpdateIndustryRequest r, CancellationToken ct = default)
        {
            var e = await _repo.GetByIdAsync(r.Id, ct);
            if (e == null) return null;

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

		private static IndustryDto Map(Industry e) => new IndustryDto
		{
			Id = e.Id,
			Name = e.Name
		};
	}
}
