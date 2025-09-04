using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Evidences
{
    public record CreateEvidenceRequest(
        [property: Required] Guid CustomerId,
        [property: Required, StringLength(64, MinimumLength = 1), Display(Name = "Quelle")] string Source,
        [property: Required, Display(Name = "Erfasst am")] DateTimeOffset CollectedAt,
        [property: Display(Name = "Ablageort"), StringLength(1024)] string? Location = null,
        [property: Display(Name = "Gültig bis")] DateTimeOffset? ValidUntil = null,
        [property: Display(Name = "SHA-256"), StringLength(64)] string? HashSha256 = null,
        [property: Display(Name = "Vertraulichkeit"), StringLength(64)] string? Confidentiality = null
    );
}
