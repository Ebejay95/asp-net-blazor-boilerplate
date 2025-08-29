using System;

namespace CMC.Domain.Entities
{
	public class ToDo : ISoftDeletable
	{
		public Guid Id { get; private set; }

		// Referenzen jetzt als Guids (kein Join erforderlich)
		public Guid ControlId { get; private set; }                 // ehemals string "C001"
		public Guid? DependsOnTaskId { get; private set; }          // verweist auf anderes ToDo.Id

		// Inhalt
		public string Name { get; private set; } = string.Empty;
		public int InternalDays { get; private set; }        // int_days
		public int ExternalDays { get; private set; }        // ext_days
		public int TotalDays { get; private set; }           // = int + ext

		public DateTimeOffset? StartDate { get; private set; }
		public DateTimeOffset? EndDate { get; private set; }

		public string Status { get; private set; } = string.Empty;      // "todo", "in_progress", "done", ...
		public string Assignee { get; private set; } = string.Empty;    // frei (E-Mail, Kürzel, Team)

		// Audit
		public DateTimeOffset CreatedAt { get; private set; }
		public DateTimeOffset UpdatedAt { get; private set; }

		// Soft Delete
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public string? DeletedBy { get; set; }

		private ToDo() { }

		public ToDo(
			Guid controlId,
			string name,
			int internalDays,
			int externalDays,
			Guid? dependsOnTaskId = null,
			DateTimeOffset? startDateUtc = null,
			DateTimeOffset? endDateUtc = null,
			string? status = null,
			string? assignee = null,
			DateTimeOffset? createdAtUtc = null)
		{
			if (controlId == Guid.Empty) throw new ArgumentException("ControlId required.", nameof(controlId));
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
			if (internalDays < 0 || externalDays < 0) throw new ArgumentException("Days must be >= 0");

			Id = Guid.NewGuid();
			ControlId = controlId;
			Name = name.Trim();
			InternalDays = internalDays;
			ExternalDays = externalDays;
			TotalDays = internalDays + externalDays;

			DependsOnTaskId = dependsOnTaskId;
			StartDate = startDateUtc?.ToUniversalTime();
			EndDate = endDateUtc?.ToUniversalTime();

			Status = (status ?? string.Empty).Trim();
			Assignee = (assignee ?? string.Empty).Trim();

			CreatedAt = createdAtUtc?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
			UpdatedAt = CreatedAt;

			if (StartDate.HasValue && EndDate.HasValue && EndDate < StartDate)
				throw new ArgumentException("EndDate must be >= StartDate");
		}

		public void SetEffort(int internalDays, int externalDays)
		{
			if (internalDays < 0 || externalDays < 0) throw new ArgumentException("Days must be >= 0");
			InternalDays = internalDays;
			ExternalDays = externalDays;
			TotalDays = internalDays + externalDays;
			Touch();
		}

		public void DependOn(Guid? taskId)
		{
			DependsOnTaskId = taskId;
			Touch();
		}

		public void Schedule(DateTimeOffset? startDateUtc, DateTimeOffset? endDateUtc)
		{
			var start = startDateUtc?.ToUniversalTime();
			var end = endDateUtc?.ToUniversalTime();
			if (start.HasValue && end.HasValue && end < start)
				throw new ArgumentException("EndDate must be >= StartDate");

			StartDate = start;
			EndDate = end;
			Touch();
		}

		public void SetStatus(string? status)
		{
			Status = (status ?? string.Empty).Trim();
			Touch();
		}

		public void Assign(string? assignee)
		{
			Assignee = (assignee ?? string.Empty).Trim();
			Touch();
		}

		public void MarkDone(DateTimeOffset? finishedAtUtc = null)
		{
			Status = "done";
			EndDate = finishedAtUtc?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
			Touch();
		}

		public void Delete(string? deletedBy = null)
		{
			if (!IsDeleted)
			{
				IsDeleted = true;
				DeletedAt = DateTimeOffset.UtcNow;
				DeletedBy = deletedBy;
			}
		}

		public void Restore()
		{
			if (IsDeleted)
			{
				IsDeleted = false;
				DeletedAt = null;
				DeletedBy = null;
				Touch();
			}
		}

		private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
	}
}
