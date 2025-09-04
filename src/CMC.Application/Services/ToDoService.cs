using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Contracts.ToDos;
using CMC.Domain.Entities;

namespace CMC.Application.Services
{
	public class ToDoService
	{
		private readonly IToDoRepository _repo;
		private readonly ILibraryControlRepository _libRepo;

		public ToDoService(IToDoRepository repo, ILibraryControlRepository libRepo)
		{
			_repo = repo ?? throw new ArgumentNullException(nameof(repo));
			_libRepo = libRepo ?? throw new ArgumentNullException(nameof(libRepo));
		}

		// Mappt UI/DTO-Tag -> Domain-Enum
		private static ToDoStatus ParseStatusTag(string? tag)
			=> ToDoStatusExtensions.FromTag(tag);

		public async Task<ToDoDto> CreateAsync(CreateToDoRequest r, CancellationToken ct = default)
		{
			// CreateToDoRequest.Status ist ein string? (Tag)
			var statusEnum = ParseStatusTag(r.Status);

			var t = new ToDo(
				r.ControlId,
				r.Name,
				r.InternalDays,
				r.ExternalDays,
				r.DependsOnTaskId,
				r.StartDate,
				r.EndDate,
				statusEnum,
				r.Assignee
			);

			await _repo.AddAsync(t, ct);
			return Map(t);
		}

		public async Task<ToDoDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
		{
			var t = await _repo.GetByIdAsync(id, ct);
			return t != null ? Map(t) : null;
		}

		public async Task<List<ToDoDto>> GetByControlAsync(Guid controlId, CancellationToken ct = default)
		{
			var items = await _repo.GetByControlIdAsync(controlId, ct);
			return items.Select(Map).ToList();
		}

		public async Task<ToDoDto?> UpdateAsync(UpdateToDoRequest r, CancellationToken ct = default)
		{
			var t = await _repo.GetByIdAsync(r.Id, ct);
			if (t == null) return null;

			t.SetEffort(r.InternalDays, r.ExternalDays);
			t.DependOn(r.DependsOnTaskId);
			t.Schedule(r.StartDate, r.EndDate);

			// UpdateToDoRequest hat StatusTag (string?) – nur setzen, wenn vorhanden
			if (!string.IsNullOrWhiteSpace(r.StatusTag))
				t.SetStatus(ParseStatusTag(r.StatusTag));

			t.Assign(r.Assignee);

			await _repo.UpdateAsync(t, ct);
			return Map(t);
		}

		public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
		{
			var t = await _repo.GetByIdAsync(id, ct);
			if (t == null) return false;
			await _repo.DeleteAsync(t, ct);
			return true;
		}

		public async Task<ToDoDto?> MarkDoneAsync(Guid id, DateTimeOffset? finishedAtUtc, CancellationToken ct = default)
		{
			var t = await _repo.GetByIdAsync(id, ct);
			if (t == null) return null;
			t.MarkDone(finishedAtUtc);
			await _repo.UpdateAsync(t, ct);
			return Map(t);
		}

		// Bulk aus LibraryControls (Guid-only)
		public async Task<List<ToDoDto>> CreateFromLibraryControlsAsync(
			IEnumerable<Guid> libraryControlIds,
			DateTimeOffset? startDateUtc,
			string? status,
			string? assignee,
			CancellationToken ct = default)
		{
			var libs = await _libRepo.GetByIdsAsync(libraryControlIds, ct);
			var todos = ToDoFactory.FromLibraryControls(libs, startDateUtc, status, assignee);
			await _repo.AddRangeAsync(todos, ct);
			return todos.Select(Map).ToList();
		}

		private static ToDoDto Map(ToDo t) => new ToDoDto
		{
			Id = t.Id,
			ControlId = t.ControlId,
			DependsOnTaskId = t.DependsOnTaskId,
			Name = t.Name,
			InternalDays = t.InternalDays,
			ExternalDays = t.ExternalDays,
			TotalDays = t.TotalDays,
			StartDate = t.StartDate,
			EndDate = t.EndDate,
			// Konsistent zum UI: Tag ausgeben
			Status = t.Status.ToTag(),
			Assignee = t.Assignee,
			CreatedAt = t.CreatedAt,
			UpdatedAt = t.UpdatedAt
		};

		public async Task<List<ToDoDto>> GetAllAsync(CancellationToken ct = default)
		{
			var items = await _repo.GetAllAsync(ct);
			return items.Select(Map).ToList();
		}
	}
}
