using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Controls
{
	public class ControlDto
	{
		[ScaffoldColumn(false)]
		public Guid Id { get; set; }

		[ScaffoldColumn(false)]
		public Guid CustomerId { get; set; }

		[ScaffoldColumn(false)]
		public Guid LibraryControlId { get; set; }

		[Display(Name = "Evidence")]
		public Guid? EvidenceId { get; set; }

		[Display(Name = "Umgesetzt")]
		public bool Implemented { get; set; }

		[Range(0, 1)]
		[Display(Name = "Coverage")]
		public decimal Coverage { get; set; }

		[Display(Name = "Maturity")]
		public int Maturity { get; set; }

		[Range(0, 1)]
		[Display(Name = "Evidence Weight")]
		public decimal EvidenceWeight { get; set; }

		[Range(0, 1)]
		[Display(Name = "Freshness")]
		public decimal Freshness { get; set; }

		[Display(Name = "Kosten (EUR)"), DisplayFormat(DataFormatString = "{0:C}")]
		public decimal CostTotalEur { get; set; }

		[Display(Name = "ΔEAL (EUR)"), DisplayFormat(DataFormatString = "{0:C}")]
		public decimal DeltaEalEur { get; set; }

		[Display(Name = "Score")]
		public decimal Score { get; set; }

		[Display(Name = "Status")]
		public string Status { get; set; } = string.Empty;

		[Display(Name = "Fällig am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset? DueDate { get; set; }

		[Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTime CreatedAt { get; set; }

		[Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTime UpdatedAt { get; set; }
	}
}
