using System;
using System.ComponentModel.DataAnnotations;
using CMC.Contracts.Common;

namespace CMC.Contracts.ToDos
{
	public class ToDoDto
	{
		[ScaffoldColumn(false)]
		public Guid Id { get; set; }

		// 👉 direkt auswählbar
		[Display(Name = "Control")]
		public Guid ControlId { get; set; }

		[Display(Name = "Abhängig von Task-ID")]
		public Guid? DependsOnTaskId { get; set; }

		[Display(Name = "Name")]
		public string Name { get; set; } = string.Empty;

		[Display(Name = "Interne Tage")]
		public int InternalDays { get; set; }

		[Display(Name = "Externe Tage")]
		public int ExternalDays { get; set; }

		[Display(Name = "Gesamt Tage")]
		public int TotalDays { get; set; }

		[Display(Name = "Start"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset? StartDate { get; set; }

		[Display(Name = "Ende"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset? EndDate { get; set; }

		// 👉 Anzeige im Grid
		[EditorHidden]
		[Display(Name = "Status (Label)")]
		public string Status { get; set; } = string.Empty;

		// 👉 Auswahl im Editor (mappt auf UpdateToDoRequest.StatusTag)
		[SelectFrom("CMC.Contracts.ToDos.ToDoStatuses.Statuses")]
		[Display(Name = "Status")]
		public string? StatusTag { get; set; }

		[Display(Name = "Zuständig")]
		public string Assignee { get; set; } = string.Empty;

		[Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset CreatedAt { get; set; }

		[Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset UpdatedAt { get; set; }
	}
}
