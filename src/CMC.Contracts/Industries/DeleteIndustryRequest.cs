using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Industries
{
	public record DeleteIndustryRequest(
		[property: Required]
		Guid Id
	);
}
