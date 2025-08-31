using System;
using System.ComponentModel.DataAnnotations;
using CMC.Contracts.Common;

namespace CMC.Contracts.ToDos
{
    public record UpdateToDoRequest(
        [property: Required] Guid Id,
        [property: Range(0, int.MaxValue)] int InternalDays,
        [property: Range(0, int.MaxValue)] int ExternalDays,
        Guid? DependsOnTaskId,
        DateTimeOffset? StartDate,
        DateTimeOffset? EndDate,

        // Tag statt freiem String; Server mappt via ToDoStatusExtensions.FromTag(...)
        [property: SelectFrom("CMC.Contracts.ToDos.ToDoStatuses.Statuses")]
        string? StatusTag,

        string? Assignee
    );
}
