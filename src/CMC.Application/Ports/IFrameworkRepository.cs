using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CMC.Domain.Entities;

namespace CMC.Application.Ports;

/// <summary>
/// Repository interface for managing Framework entities.
/// Provides CRUD operations and query helpers.
/// </summary>
public interface IFrameworkRepository
{
	// =============================================================================
	// READ Operations
	// =============================================================================
	Task<Framework?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<List<Framework>> GetAllAsync(CancellationToken cancellationToken = default);
	Task<Framework?> GetByNameAndVersionAsync(string name, string version, CancellationToken cancellationToken = default);
	Task<List<Framework>> GetByIndustryAsync(string industry, CancellationToken cancellationToken = default);

	// =============================================================================
	// CREATE/UPDATE/DELETE
	// =============================================================================
	Task AddAsync(Framework framework, CancellationToken cancellationToken = default);
	Task UpdateAsync(Framework framework, CancellationToken cancellationToken = default);
	Task DeleteAsync(Framework framework, CancellationToken cancellationToken = default);

	// =============================================================================
	// BUSINESS
	// =============================================================================
	/// <summary>
	/// Checks if a framework with the same name+version exists.
	/// </summary>
	Task<bool> ExistsAsync(string name, string version, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
