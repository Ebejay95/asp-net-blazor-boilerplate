using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.LibraryControls
{
	public class LibraryControlDto
	{
		[ScaffoldColumn(false)]
		public Guid Id { get; set; }

		[Display(Name = "Name")]
		public string Name { get; set; } = string.Empty;

		[Display(Name = "Tag")]
		public string Tag { get; set; } = string.Empty;

		[Display(Name = "Capex (EUR)"), DisplayFormat(DataFormatString = "{0:C}")]
		public decimal CapexEur { get; set; }

		[Display(Name = "Opex/Jahr (EUR)"), DisplayFormat(DataFormatString = "{0:C}")]
		public decimal OpexYearEur { get; set; }

		[Display(Name = "Interne Tage")]
		public int InternalDays { get; set; }

		[Display(Name = "Externe Tage")]
		public int ExternalDays { get; set; }

		[Display(Name = "Gesamt Tage")]
		public int TotalDays { get; set; }

		[Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTime CreatedAt { get; set; }

		[Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTime UpdatedAt { get; set; }
	}
}
