using System;

namespace CMC.Domain.Entities
{
	/// <summary>
	/// Kanonische Statuswerte für ToDos (Domain-Ebene).
	/// </summary>
	public enum ToDoStatus
	{
		Todo = 0,
		InProgress = 1,
		Done = 2,
		Blocked = 3,
		Canceled = 4
	}

	public static class ToDoStatusExtensions
	{
		/// <summary>
		/// Optional: String-Tags für UI/DTO-Mapping.
		/// </summary>
		public static string ToTag(this ToDoStatus s) => s switch
		{
			ToDoStatus.Todo => "todo",
			ToDoStatus.InProgress => "in_progress",
			ToDoStatus.Done => "done",
			ToDoStatus.Blocked => "blocked",
			ToDoStatus.Canceled => "canceled",
			_ => "todo"
		};

		public static ToDoStatus FromTag(string? tag) => (tag ?? "").Trim().ToLowerInvariant() switch
		{
			"in_progress" => ToDoStatus.InProgress,
			"done" => ToDoStatus.Done,
			"blocked" => ToDoStatus.Blocked,
			"canceled" => ToDoStatus.Canceled,
			_ => ToDoStatus.Todo
		};
	}
}
