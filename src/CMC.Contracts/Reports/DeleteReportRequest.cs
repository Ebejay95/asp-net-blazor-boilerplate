using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Reports
{
	public record DeleteReportRequest(
		[property: Required]
		Guid Id
	);
}
