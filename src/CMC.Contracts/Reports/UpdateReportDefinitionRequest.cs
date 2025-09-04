using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Reports
{
	public record UpdateReportDefinitionRequest(
		[property: Required] Guid Id,
		[property: Required, StringLength(200, MinimumLength = 1), Display(Name = "Name")]
		string Name,
		[property: Required, StringLength(50, MinimumLength = 1), Display(Name = "Art")]
		string Kind,
		[property: Range(0, int.MaxValue), Display(Name = "Fenster (Tage)")]
		int WindowDays,
		[property: Display(Name = "Sektionen")]
		string? Sections = null
	);
}
