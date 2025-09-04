using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Industries
{
	public record UpdateIndustryRequest(
		[property: Required] Guid Id,
		[property: Required, StringLength(200, MinimumLength = 1), Display(Name = "Branche")]
		string Name
	);
}
