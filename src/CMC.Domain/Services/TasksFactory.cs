using System;
using System.Collections.Generic;

namespace CMC.Domain.Entities
{
	public static class TasksFactory
	{
		// Erzeugt EIN ToDo aus einem LibraryControl.
		// Referenz jetzt via Guid (lib.Id) statt sichtbarem String-Code.
		public static ToDo FromLibraryControl(
			LibraryControl lib,
			DateTimeOffset? startDateUtc = null,
			string? status = "todo",
			string? assignee = null,
			Guid? dependsOnTaskId = null)
		{
			if (lib == null) throw new ArgumentNullException(nameof(lib));

			return new ToDo(
				controlId: lib.Id,
				name: lib.Name,
				internalDays: lib.InternalDays,
				externalDays: lib.ExternalDays,
				dependsOnTaskId: dependsOnTaskId,
				startDateUtc: startDateUtc,
				endDateUtc: null,
				status: status,
				assignee: assignee
			);
		}

		// Bulk: mehrere ToDos aus einer Liste von LibraryControls
		public static List<ToDo> FromLibraryControls(
			IEnumerable<LibraryControl> items,
			DateTimeOffset? startDateUtc = null,
			string? status = "todo",
			string? assignee = null)
		{
			var result = new List<ToDo>();
			foreach (var lib in items)
			{
				result.Add(FromLibraryControl(lib, startDateUtc, status, assignee));
			}
			return result;
		}
	}
}
