using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.LibraryControls
{
	public record CreateLibraryControlRequest(
		[property: Required, StringLength(200, MinimumLength = 1), Display(Name = "Name")]
		string Name,
		[property: Range(typeof(decimal), "0", "79228162514264337593543950335"), Display(Name = "Capex (EUR)")]
		decimal CapexEur,
		[property: Range(typeof(decimal), "0", "79228162514264337593543950335"), Display(Name = "Opex/Jahr (EUR)")]
		decimal OpexYearEur,
		[property: Range(0, int.MaxValue), Display(Name = "Interne Tage")]
		int InternalDays,
		[property: Range(0, int.MaxValue), Display(Name = "Externe Tage")]
		int ExternalDays,
		int? TotalDays = null,

		// M:N: Tags + Branchen als Guid-Arrays
		[property: Display(Name = "Tags")]
		IReadOnlyList<Guid>? TagIds = null,
		[property: Display(Name = "Branchen")]
		IReadOnlyList<Guid>? IndustryIds = null
	);
}
