using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.ToDos
{
	public record UpdateToDoRequest(
		[property: Required] Guid Id,
		[property: Range(0, int.MaxValue)] int InternalDays,
		[property: Range(0, int.MaxValue)] int ExternalDays,
		Guid? DependsOnTaskId,
		DateTimeOffset? StartDate,
		DateTimeOffset? EndDate,
		string? Status,
		string? Assignee
	);
}
