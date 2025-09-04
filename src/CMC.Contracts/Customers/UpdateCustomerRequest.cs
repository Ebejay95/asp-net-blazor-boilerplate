using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Customers
{
	/// <summary>Update Customer</summary>
	public record UpdateCustomerRequest(
		[property: Required] Guid Id,
		[property: Required, StringLength(200, MinimumLength = 1), Display(Name = "Firmenname")] string Name,
		// NULL = unverändert, leere Liste = alle entfernen
		[property: Display(Name = "Branchen")] IReadOnlyList<Guid>? IndustryIds,
		[property: Required, Range(0, int.MaxValue, ErrorMessage = "Anzahl Mitarbeiter muss 0 oder größer sein"), Display(Name = "Anzahl Mitarbeiter")] int EmployeeCount,
		[property: Required, Range(0, double.MaxValue, ErrorMessage = "Jahresumsatz muss 0 oder größer sein"), Display(Name = "Jahresumsatz")] decimal RevenuePerYear,
		[property: Display(Name = "Aktiv")] bool? IsActive = null
	);
}
