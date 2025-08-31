using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.ToDos
{
	public class ToDoDto
	{
		[ScaffoldColumn(false)]
		public Guid Id { get; set; }

		[Display(Name = "Control-ID")]
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

		[Display(Name = "Status")]
		public string Status { get; set; } = string.Empty;

		[Display(Name = "Zuständig")]
		public string Assignee { get; set; } = string.Empty;

		[Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset CreatedAt { get; set; }

		[Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset UpdatedAt { get; set; }
	}
}
