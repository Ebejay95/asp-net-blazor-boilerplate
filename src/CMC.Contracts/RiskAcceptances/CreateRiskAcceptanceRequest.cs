using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.RiskAcceptances
{
	public record CreateRiskAcceptanceRequest(
		[property: Required, Display(Name = "Kunde")]
		Guid CustomerId,

		[property: Required, Display(Name = "Control-ID")]
		Guid ControlId,

		[property: Required, StringLength(2000, MinimumLength = 1), Display(Name = "Begründung")]
		string Reason,

		[property: Required, StringLength(200, MinimumLength = 1), Display(Name = "Akzeptiert von")]
		string RiskAcceptedBy,

		[property: Required, Display(Name = "Gültig bis")]
		DateTimeOffset ExpiresAt
	);
}
