using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.LibraryScenarios
{
	public record DeleteLibraryScenarioRequest(
		[property: Required]
		Guid Id
	);
}
