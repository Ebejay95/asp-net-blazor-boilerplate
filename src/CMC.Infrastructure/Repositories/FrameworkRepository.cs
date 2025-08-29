using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories;

public class FrameworkRepository : IFrameworkRepository
{
	private readonly AppDbContext _context;

	public FrameworkRepository(AppDbContext context)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
	}

	public async Task<Framework?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		=> await _context.Frameworks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

	public async Task<List<Framework>> GetAllAsync(CancellationToken cancellationToken = default)
		=> await _context.Frameworks.AsNoTracking().OrderBy(x => x.Name).ThenBy(x => x.Version).ToListAsync(cancellationToken);

	public async Task<Framework?> GetByNameAndVersionAsync(string name, string version, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
		if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException(nameof(version));
		return await _context.Frameworks.FirstOrDefaultAsync(x => x.Name == name && x.Version == version, cancellationToken);
	}

	/// <summary>
	/// NEU: Filter über Join-Tabelle FrameworkIndustries (Framework hat kein direktes Industry-Property mehr).
	/// </summary>
	public async Task<List<Framework>> GetByIndustryAsync(string industry, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(industry)) throw new ArgumentException(nameof(industry));

		var industryIds = _context.Industries
			.Where(i => i.Name == industry)
			.Select(i => i.Id);

		return await _context.FrameworkIndustries
			.Where(fi => industryIds.Contains(fi.IndustryId))
			.Select(fi => fi.Framework!)
			.AsNoTracking()
			.OrderBy(x => x.Name).ThenBy(x => x.Version)
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
		_context.Frameworks.Remove(framework);
		await _context.SaveChangesAsync(cancellationToken);
	}

	public async Task<bool> ExistsAsync(string name, string version, Guid? excludeId = null, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
		if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException(nameof(version));

		var q = _context.Frameworks.Where(x => x.Name == name && x.Version == version);
		if (excludeId.HasValue) q = q.Where(x => x.Id != excludeId.Value);
		return await q.AnyAsync(cancellationToken);
	}
}
