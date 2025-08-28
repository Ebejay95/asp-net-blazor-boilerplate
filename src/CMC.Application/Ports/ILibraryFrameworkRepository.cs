using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CMC.Domain.Entities;

namespace CMC.Application.Ports;

/// <summary>
/// Repository interface for managing LibraryFramework entities.
/// Provides CRUD operations and query helpers.
/// </summary>
public interface ILibraryFrameworkRepository
{
	// =============================================================================
	// READ Operations
	// =============================================================================
	Task<LibraryFramework?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<List<LibraryFramework>> GetAllAsync(CancellationToken cancellationToken = default);
	Task<LibraryFramework?> GetByNameAndVersionAsync(string name, string version, CancellationToken cancellationToken = default);
	Task<List<LibraryFramework>> GetByIndustryAsync(string industry, CancellationToken cancellationToken = default);

	// =============================================================================
	// CREATE/UPDATE/DELETE
	// =============================================================================
	Task AddAsync(LibraryFramework framework, CancellationToken cancellationToken = default);
	Task UpdateAsync(LibraryFramework framework, CancellationToken cancellationToken = default);
	Task DeleteAsync(LibraryFramework framework, CancellationToken cancellationToken = default);

	// =============================================================================
	// BUSINESS
	// =============================================================================
	/// <summary>
	/// Checks if a framework with the same name+version exists.
	/// </summary>
	Task<bool> ExistsAsync(string name, string version, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
