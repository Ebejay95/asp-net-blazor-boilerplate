using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Reports
{
	public class ReportDefinitionDto
	{
		[ScaffoldColumn(false)]
		public Guid Id { get; set; }

		[Display(Name = "Kunde")]
		public Guid CustomerId { get; init; }

		[Display(Name = "Name")]
		public string Name { get; set; } = string.Empty;

		[Display(Name = "Art")]
		public string Kind { get; set; } = string.Empty;

		[Display(Name = "Fenster (Tage)")]
		public int WindowDays { get; set; }

		[Display(Name = "Sektionen")]
		public string Sections { get; set; } = string.Empty;

		[Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset CreatedAt { get; set; }

		[Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset UpdatedAt { get; set; }
	}
}
