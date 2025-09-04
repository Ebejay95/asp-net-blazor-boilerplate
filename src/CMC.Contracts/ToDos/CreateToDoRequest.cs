using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.ToDos
{
	public record CreateToDoRequest(
		[property: Required] Guid ControlId,
		[property: Required, StringLength(200, MinimumLength = 1)] string Name,
		[property: Range(0, int.MaxValue)] int InternalDays,
		[property: Range(0, int.MaxValue)] int ExternalDays,
		Guid? DependsOnTaskId = null,
		DateTimeOffset? StartDate = null,
		DateTimeOffset? EndDate = null,
		string? Status = null,
		string? Assignee = null
	);
}
