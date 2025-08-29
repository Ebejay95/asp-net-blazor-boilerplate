using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.LibraryControls
{
	public record UpdateLibraryControlRequest(
		[property: Required] Guid Id,
		[property: Required, StringLength(64, MinimumLength = 1), Display(Name = "Tag")]
		string Tag,
		[property: Range(0, int.MaxValue), Display(Name = "Interne Tage")]
		int InternalDays,
		[property: Range(0, int.MaxValue), Display(Name = "Externe Tage")]
		int ExternalDays,
		int? TotalDays,
		[property: Range(0, double.MaxValue), Display(Name = "Capex (EUR)")]
		decimal CapexEur,
		[property: Range(0, double.MaxValue), Display(Name = "Opex/Jahr (EUR)")]
		decimal OpexYearEur
	);
}
