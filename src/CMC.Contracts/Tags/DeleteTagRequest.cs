using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Tags
{
	public record DeleteTagRequest(
		[property: Required]
		Guid Id
	);
}
