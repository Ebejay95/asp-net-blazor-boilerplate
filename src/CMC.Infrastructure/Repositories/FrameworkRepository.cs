using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories
{
	public class FrameworkRepository : IFrameworkRepository
	{
		private readonly AppDbContext _context;

		public FrameworkRepository(AppDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public Task<Framework?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
			=> _context.Frameworks
				.Include(f => f.IndustryLinks)
					.ThenInclude(li => li.Industry)
				.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

		public Task<List<Framework>> GetAllAsync(CancellationToken cancellationToken = default)
			=> _context.Frameworks
				.Include(f => f.IndustryLinks)
					.ThenInclude(li => li.Industry)
				.AsNoTracking()
				.OrderBy(x => x.Name)
				.ThenBy(x => x.Version)
				.ToListAsync(cancellationToken);

		public Task<Framework?> GetByNameAndVersionAsync(string name, string version, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
			if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException(nameof(version));

			return _context.Frameworks
				.Include(f => f.IndustryLinks)
					.ThenInclude(li => li.Industry)
				.FirstOrDefaultAsync(x => x.Name == name && x.Version == version, cancellationToken);
		}

		public Task<List<Framework>> GetByIndustryIdAsync(Guid industryId, CancellationToken cancellationToken = default)
		{
			if (industryId == Guid.Empty) throw new ArgumentException(nameof(industryId));

			return _context.Frameworks
				.Include(f => f.IndustryLinks)
					.ThenInclude(li => li.Industry)
				.Where(f => f.IndustryLinks.Any(li => li.IndustryId == industryId))
				.AsNoTracking()
				.OrderBy(x => x.Name)
				.ThenBy(x => x.Version)
				.ToListAsync(cancellationToken);
		}

		public async Task AddAsync(Framework framework, CancellationToken cancellationToken = default)
		{
			if (framework is null) throw new ArgumentNullException(nameof(framework));
			_context.Frameworks.Add(framework);
			await _context.SaveChangesAsync(cancellationToken);
		}

		public async Task UpdateAsync(Framework framework, CancellationToken cancellationToken = default)
		{
			if (framework is null) throw new ArgumentNullException(nameof(framework));
			_context.Frameworks.Update(framework);
			await _context.SaveChangesAsync(cancellationToken);
		}

		public async Task DeleteAsync(Framework framework, CancellationToken cancellationToken = default)
		{
			if (framework is null) throw new ArgumentNullException(nameof(framework));
			framework.Delete();
			_context.Frameworks.Update(framework);
			await _context.SaveChangesAsync(cancellationToken);
		}

		public Task<bool> ExistsAsync(string name, string version, Guid? excludeId = null, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
			if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException(nameof(version));

			var q = _context.Frameworks.Where(x => x.Name == name && x.Version == version);
			if (excludeId.HasValue) q = q.Where(x => x.Id != excludeId.Value);
			return q.AnyAsync(cancellationToken);
		}
	}
}
