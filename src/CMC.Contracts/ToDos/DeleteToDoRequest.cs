using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.ToDos
{
	public record DeleteToDoRequest(
		[property: Required]
		Guid Id
	);
}
