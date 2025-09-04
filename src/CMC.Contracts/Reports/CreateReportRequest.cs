using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Reports
{
	public record CreateReportRequest(
		[property: Required, Display(Name = "Definition")]
		Guid DefinitionId,
		[property: Required, Display(Name = "Zeitraum von")]
		DateTimeOffset PeriodStart,
		[property: Required, Display(Name = "Zeitraum bis")]
		DateTimeOffset PeriodEnd,
		[property: Required, Display(Name = "Generiert am")]
		DateTimeOffset GeneratedAt,
		[property: Display(Name = "Eingefroren")]
		bool Frozen = false,
		[property: Display(Name = "Kunde")]
		Guid? CustomerId = null
	);
}
