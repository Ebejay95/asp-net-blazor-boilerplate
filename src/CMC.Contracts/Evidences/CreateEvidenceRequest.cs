using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Evidences
{
	public record CreateEvidenceRequest(
		[property: Required] Guid CustomerId,
		[property: Required, StringLength(100, MinimumLength = 1), Display(Name = "Quelle")]
		string Source,
		[property: Required, Display(Name = "Erfasst am")]
		DateTimeOffset CollectedAt,
		[property: Display(Name = "Ablageort")]
		string? Location = null,
		[property: Display(Name = "Gültig bis")]
		DateTimeOffset? ValidUntil = null,
		[property: Display(Name = "SHA-256")]
		string? HashSha256 = null,
		[property: Display(Name = "Vertraulichkeit")]
		string? Confidentiality = null
	);
}
