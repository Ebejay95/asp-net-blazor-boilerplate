using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Contracts.LibraryFrameworks;
using CMC.Domain.Common;
using CMC.Domain.Entities;

namespace CMC.Application.Services;

/// <summary>
/// Application service for managing LibraryFramework-related operations.
/// </summary>
public class LibraryFrameworkService
{
    #region Fields

    private readonly ILibraryFrameworkRepository _repository;

    #endregion

    #region Constructor

    public LibraryFrameworkService(ILibraryFrameworkRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    #endregion

    #region CREATE

    public async Task<LibraryFrameworkDto> CreateAsync(CreateLibraryFrameworkRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (await _repository.ExistsAsync(request.Name, request.Version, null, cancellationToken))
            throw new DomainException("Framework with this name and version already exists");

        var entity = new LibraryFramework(
            name: request.Name,
            version: request.Version,
            industry: request.Industry
        );

        await _repository.AddAsync(entity, cancellationToken);

        return MapToDto(entity);
    }

    #endregion

    #region READ

    public async Task<LibraryFrameworkDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<List<LibraryFrameworkDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<List<LibraryFrameworkDto>> GetByIndustryAsync(string industry, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(industry))
            throw new ArgumentException("Industry cannot be null or empty", nameof(industry));

        var items = await _repository.GetByIndustryAsync(industry, cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    #endregion

    #region UPDATE

    public async Task<LibraryFrameworkDto?> UpdateAsync(UpdateLibraryFrameworkRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null) return null;

        if (await _repository.ExistsAsync(request.Name, request.Version, request.Id, cancellationToken))
            throw new DomainException("Framework with this name and version already exists");

        entity.Rename(request.Name);
        entity.SetVersion(request.Version);
        entity.SetIndustry(request.Industry);

        await _repository.UpdateAsync(entity, cancellationToken);

        var updated = await _repository.GetByIdAsync(entity.Id, cancellationToken);
        return updated != null ? MapToDto(updated) : null;
    }

    #endregion

    #region DELETE

    public async Task<bool> DeleteAsync(DeleteLibraryFrameworkRequest request, CancellationToken cancellationToken = default)
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

    #endregion

    #region Mapping

    private static LibraryFrameworkDto MapToDto(LibraryFramework entity) => new(
        entity.Id,
        entity.Name,
        entity.Version,
        entity.Industry,
        // Falls die Entity-Eigenschaften DateTimeOffset sind, konvertieren wir auf DateTime (UTC):
        entity.CreatedAt.UtcDateTime,
        entity.UpdatedAt.UtcDateTime
    );

    #endregion
}
