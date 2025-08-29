using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Controls
{
	public record LinkEvidenceRequest(
		[property: Required] Guid ControlId,
		Guid? EvidenceId
	);
}
