using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Customers
{
	/// <summary>Create Customer</summary>
	public record CreateCustomerRequest(
		[property: Required, StringLength(200, MinimumLength = 1), Display(Name = "Firmenname")]
		string Name,
		// M:N: mehrere Branchen
		[property: Display(Name = "Branchen")]
		Guid[] IndustryIds,
		[property: Required, Range(0, int.MaxValue, ErrorMessage = "Anzahl Mitarbeiter muss 0 oder größer sein"), Display(Name = "Anzahl Mitarbeiter")]
		int EmployeeCount,
		[property: Required, Range(0, double.MaxValue, ErrorMessage = "Jahresumsatz muss 0 oder größer sein"), Display(Name = "Jahresumsatz")]
		decimal RevenuePerYear
	);
}
