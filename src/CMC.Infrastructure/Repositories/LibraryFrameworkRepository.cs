using CMC.Application.Ports;
using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Repositories;

public class LibraryFrameworkRepository : ILibraryFrameworkRepository
{
	private readonly AppDbContext _context;

	public LibraryFrameworkRepository(AppDbContext context)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
	}

	public async Task<LibraryFramework?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		=> await _context.LibraryFrameworks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

	public async Task<List<LibraryFramework>> GetAllAsync(CancellationToken cancellationToken = default)
		=> await _context.LibraryFrameworks.AsNoTracking().OrderBy(x => x.Name).ThenBy(x => x.Version).ToListAsync(cancellationToken);

	public async Task<LibraryFramework?> GetByNameAndVersionAsync(string name, string version, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
		if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException(nameof(version));
		return await _context.LibraryFrameworks.FirstOrDefaultAsync(x => x.Name == name && x.Version == version, cancellationToken);
	}

	public async Task<List<LibraryFramework>> GetByIndustryAsync(string industry, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(industry)) throw new ArgumentException(nameof(industry));
		return await _context.LibraryFrameworks.Where(x => x.Industry == industry)
			.AsNoTracking().OrderBy(x => x.Name).ThenBy(x => x.Version).ToListAsync(cancellationToken);
	}

	public async Task AddAsync(LibraryFramework framework, CancellationToken cancellationToken = default)
	{
		if (framework is null) throw new ArgumentNullException(nameof(framework));
		_context.LibraryFrameworks.Add(framework);
		await _context.SaveChangesAsync(cancellationToken);
	}

	public async Task UpdateAsync(LibraryFramework framework, CancellationToken cancellationToken = default)
	{
		if (framework is null) throw new ArgumentNullException(nameof(framework));
		_context.LibraryFrameworks.Update(framework);
		await _context.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteAsync(LibraryFramework framework, CancellationToken cancellationToken = default)
	{
		if (framework is null) throw new ArgumentNullException(nameof(framework));
		_context.LibraryFrameworks.Remove(framework);
		await _context.SaveChangesAsync(cancellationToken);
	}

	public async Task<bool> ExistsAsync(string name, string version, Guid? excludeId = null, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
		if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException(nameof(version));

		var q = _context.LibraryFrameworks.Where(x => x.Name == name && x.Version == version);
		if (excludeId.HasValue) q = q.Where(x => x.Id != excludeId.Value);
		return await q.AnyAsync(cancellationToken);
	}
}
