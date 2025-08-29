using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.RiskAcceptances
{
	public record DeleteRiskAcceptanceRequest(
		[property: Required]
		Guid Id
	);
}
