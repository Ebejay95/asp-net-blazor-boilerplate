using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.RiskAcceptances
{
	public record UpdateRiskAcceptanceRequest(
		[property: Required]
		Guid Id,

		// Optional: Referenzen ändern (setzt Entity.SetRefs)
		[property: StringLength(100, MinimumLength = 1), Display(Name = "Kunde")]
		Guid CustomerId,

		[property: StringLength(64, MinimumLength = 1), Display(Name = "Control-ID")]
		Guid ControlId,

		// Inhalte
		[property: Required, StringLength(2000, MinimumLength = 1), Display(Name = "Begründung")]
		string Reason = "",

		[property: Required, StringLength(200, MinimumLength = 1), Display(Name = "Akzeptiert von")]
		string RiskAcceptedBy = "",

		[property: Required, Display(Name = "Gültig bis")]
		DateTimeOffset ExpiresAt = default
	);
}
