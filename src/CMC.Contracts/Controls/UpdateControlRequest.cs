using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Controls
{
	public record UpdateControlRequest(
		[property: Required] Guid Id,
		[property: Display(Name = "Umgesetzt")] bool Implemented,
		[property: Range(0, 1)] decimal Coverage,
		[property: Display(Name = "Maturity")] int Maturity,
		[property: Range(0, 1)] decimal EvidenceWeight,
		[property: Range(0, 1)] decimal Freshness,
		[property: Display(Name = "Kosten (EUR)")] decimal CostTotalEur,
		[property: Display(Name = "ΔEAL (EUR)")] decimal DeltaEalEur,
		[property: Display(Name = "Score")] decimal Score,
		[property: Display(Name = "Status")] string? Status,
		DateTimeOffset? DueDate,
		Guid? EvidenceId
	);
}
