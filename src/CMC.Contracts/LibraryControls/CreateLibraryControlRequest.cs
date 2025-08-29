using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.LibraryControls
{
	public record CreateLibraryControlRequest(
		[property: Required, StringLength(200, MinimumLength = 1), Display(Name = "Name")]
		string Name,
		[property: Required, StringLength(64, MinimumLength = 1), Display(Name = "Tag")]
		string Tag,
		[property: Range(0, double.MaxValue), Display(Name = "Capex (EUR)")]
		decimal CapexEur,
		[property: Range(0, double.MaxValue), Display(Name = "Opex/Jahr (EUR)")]
		decimal OpexYearEur,
		[property: Range(0, int.MaxValue), Display(Name = "Interne Tage")]
		int InternalDays,
		[property: Range(0, int.MaxValue), Display(Name = "Externe Tage")]
		int ExternalDays,
		int? TotalDays = null
	);
}
