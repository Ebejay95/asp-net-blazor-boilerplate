using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Evidences
{
	public record DeleteEvidenceRequest(
		[property: Required]
		Guid Id
	);
}
