using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Reports
{
	public record UpdateReportRequest(
		[property: Required] Guid Id,
		[property: Required, StringLength(100, MinimumLength = 1), Display(Name = "Definition")]
		Guid DefinitionId,
		[property: Required, Display(Name = "Zeitraum von")]
		DateTimeOffset PeriodStart,
		[property: Required, Display(Name = "Zeitraum bis")]
		DateTimeOffset PeriodEnd,
		[property: Required, Display(Name = "Generiert am")]
		DateTimeOffset GeneratedAt,
		[property: Display(Name = "Eingefroren")]
		bool? Frozen,
		[property: Display(Name = "Kunde")]
		Guid? CustomerId
	);
}
