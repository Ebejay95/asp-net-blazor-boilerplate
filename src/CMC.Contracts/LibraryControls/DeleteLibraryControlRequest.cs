using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.LibraryControls
{
	public record DeleteLibraryControlRequest(
		[property: Required]
		Guid Id
	);
}
