using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Evidences
{
	public record UpdateEvidenceRequest(
		[property: Required] Guid Id,
		[property: Required, StringLength(100, MinimumLength = 1), Display(Name = "Quelle")]
		string Source,
		[property: Display(Name = "Ablageort")]
		string? Location,
		[property: Display(Name = "Gültig bis")]
		DateTimeOffset? ValidUntil,
		[property: Display(Name = "SHA-256")]
		string? HashSha256,
		[property: Display(Name = "Vertraulichkeit")]
		string? Confidentiality
	);
}
