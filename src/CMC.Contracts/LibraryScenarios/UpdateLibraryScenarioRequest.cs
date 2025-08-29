using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.LibraryScenarios
{
	public record UpdateLibraryScenarioRequest(
		[property: Required] Guid Id,
		[property: Required, StringLength(200, MinimumLength = 1), Display(Name = "Szenario")]
		string Name,
		[property: Required, Display(Name = "Jährliche Häufigkeit")]
		decimal AnnualFrequency,
		[property: Required, Display(Name = "Impact (% Umsatz)")]
		decimal ImpactPctRevenue,
		[property: Display(Name = "Tags")]
		string? Tags = null
	);
}
