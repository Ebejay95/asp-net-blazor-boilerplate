using System;
using System.Collections.Generic;

namespace CMC.Domain.Entities
{
	public static class ToDoFactory
	{
		/// <summary>
		/// Neuer Hauptweg: ToDo aus einer konkreten Control-Instanz.
		/// </summary>
		public static ToDo FromControl(
			Control control,
			DateTimeOffset? startDateUtc = null,
			ToDoStatus status = ToDoStatus.Todo,
			string? assignee = null,
			Guid? dependsOnTaskId = null,
			int? internalDaysOverride = null,
			int? externalDaysOverride = null,
			string? nameOverride = null)
		{
			if (control == null) throw new ArgumentNullException(nameof(control));

			var name = !string.IsNullOrWhiteSpace(nameOverride)
				? nameOverride!.Trim()
				: (control.LibraryControl?.Name ?? $"Control {control.Id}");

			// Falls Aufwand nicht überschrieben: 0/0 lassen (oder aus LibraryControl übernehmen, wenn geladen)
			var intDays = internalDaysOverride ?? control.LibraryControl?.InternalDays ?? 0;
			var extDays = externalDaysOverride ?? control.LibraryControl?.ExternalDays ?? 0;

			return new ToDo(
				controlId: control.Id,
				name: name,
				internalDays: intDays,
				externalDays: extDays,
				dependsOnTaskId: dependsOnTaskId,
				startDateUtc: startDateUtc,
				endDateUtc: null,
				status: status,
				assignee: assignee
			);
		}

		/// <summary>
		/// Bulk-Erzeugung für mehrere Controls.
		/// </summary>
		public static List<ToDo> FromControls(
			IEnumerable<Control> controls,
			DateTimeOffset? startDateUtc = null,
			ToDoStatus status = ToDoStatus.Todo,
			string? assignee = null)
		{
			if (controls == null) throw new ArgumentNullException(nameof(controls));
			var result = new List<ToDo>();
			foreach (var control in controls)
			{
				if (control == null) continue;
				result.Add(FromControl(control, startDateUtc, status, assignee));
			}
			return result;
		}

		/// <summary>
		/// Alt: ToDo aus LibraryControl. Nicht mehr erlaubt, da ToDos an Control hängen sollen.
		/// </summary>
		[Obsolete("Use FromControl(Control, ...) – ToDos müssen an Control-Instanzen hängen.")]
		public static ToDo FromLibraryControl(
			LibraryControl lib,
			DateTimeOffset? startDateUtc = null,
			string? status = "todo",
			string? assignee = null,
			Guid? dependsOnTaskId = null)
		{
			throw new NotSupportedException("ToDo must reference a Control instance. Use FromControl(...) instead.");
		}

		/// <summary>
		/// Alt: Bulk aus LibraryControls. Nicht mehr erlaubt.
		/// </summary>
		[Obsolete("Use FromControls(IEnumerable<Control>, ...)")]
		public static List<ToDo> FromLibraryControls(
			IEnumerable<LibraryControl> items,
			DateTimeOffset? startDateUtc = null,
			string? status = "todo",
			string? assignee = null)
		{
			throw new NotSupportedException("ToDos must be created from Control instances. Use FromControls(...).");
		}
	}
}
