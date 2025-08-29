using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Scenarios
{
	public record DeleteScenarioRequest(
		[property: Required]
		Guid Id
	);
}
