using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Reports
{
	public class ReportDto
	{
		[ScaffoldColumn(false)]
		public Guid Id { get; set; }

		[Display(Name = "Kunde")]
		public Guid? CustomerId { get; init; }

		[Display(Name = "Definition")]
		public Guid DefinitionId { get; init; }

		[Display(Name = "Zeitraum von"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTime PeriodStart { get; set; }

		[Display(Name = "Zeitraum bis"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTime PeriodEnd { get; set; }

		[Display(Name = "Generiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTime GeneratedAt { get; set; }

		[Display(Name = "Eingefroren")]
		public bool Frozen { get; set; }

		[Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTime CreatedAt { get; set; }

		[Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTime UpdatedAt { get; set; }
	}
}
