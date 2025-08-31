using System;
using System.ComponentModel.DataAnnotations;
using CMC.Contracts.Common;

namespace CMC.Contracts.Controls
{
	public class ControlDto
	{
		[ScaffoldColumn(false)]
		public Guid Id { get; set; }

		// 👉 direkt im Formular auswählbar
		[Display(Name = "Kunde")]
		public Guid CustomerId { get; set; }

		// 👉 Vorlage (Library-Control) auswählbar
		[Display(Name = "Vorlage")]
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

		// 👉 Lesbares Label fürs Grid
		[EditorHidden]
		[Display(Name = "Status (Label)")]
		public string Status { get; set; } = string.Empty;

		// 👉 Tag-Auswahl fürs Formular (mappt auf UpdateControlRequest.StatusTag)
		[SelectFrom("CMC.Contracts.Controls.ControlStatuses.Statuses")]
		[Display(Name = "Status")]
		public string? StatusTag { get; set; }

		[Display(Name = "Fällig am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset? DueDate { get; set; }

		[Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset CreatedAt { get; set; }

		[Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset UpdatedAt { get; set; }
	}
}
