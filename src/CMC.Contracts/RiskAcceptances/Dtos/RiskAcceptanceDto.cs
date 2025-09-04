using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.RiskAcceptances
{
	public class RiskAcceptanceDto
	{
		[ScaffoldColumn(false)]
		public Guid Id { get; set; }

		[Display(Name = "Kunde")]
		public Guid CustomerId { get; init; }

		[Display(Name = "Control-ID")]
		public Guid ControlId { get; init; }

		[Display(Name = "Begründung")]
		public string Reason { get; set; } = string.Empty;

		[Display(Name = "Akzeptiert von")]
		public string RiskAcceptedBy { get; set; } = string.Empty;

		[Display(Name = "Gültig bis"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset ExpiresAt { get; set; }

		[Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset CreatedAt { get; set; }

		[Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset UpdatedAt { get; set; }
	}
}
