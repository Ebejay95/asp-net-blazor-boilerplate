using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Controls
{
	public record CreateControlRequest(
		[property: Required] Guid CustomerId,
		[property: Required] Guid LibraryControlId,
		[property: Display(Name = "Umgesetzt")] bool Implemented,
		[property: Range(0, 1)] decimal Coverage,
		[property: Display(Name = "Maturity")] int Maturity,
		[property: Range(0, 1)] decimal EvidenceWeight,
		Guid? EvidenceId,
		[property: Range(0, 1)] decimal Freshness,
		[property: Display(Name = "Kosten (EUR)")] decimal CostTotalEur,
		[property: Display(Name = "ΔEAL (EUR)")] decimal DeltaEalEur,
		[property: Display(Name = "Score")] decimal Score,
		[property: Display(Name = "Status")] string? Status,
		[property: Display(Name = "Fällig am")] DateTimeOffset? DueDate
	);
}
