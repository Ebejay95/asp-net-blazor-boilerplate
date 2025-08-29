using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Controls
{
	public record DeleteControlRequest(
		[property: Required]
		Guid Id
	);
}
