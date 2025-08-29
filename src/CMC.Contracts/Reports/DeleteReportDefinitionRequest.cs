using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Reports
{
	public record DeleteReportDefinitionRequest(
		[property: Required]
		Guid Id
	);
}
