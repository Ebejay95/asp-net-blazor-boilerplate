using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Evidences
{
    public record UpdateEvidenceRequest(
        [property: Required] Guid Id,
        [property: Required, StringLength(64, MinimumLength = 1), Display(Name = "Quelle")] string Source,
        [property: Display(Name = "Ablageort"), StringLength(1024)] string? Location,
        [property: Display(Name = "Gültig bis")] DateTimeOffset? ValidUntil,
        [property: Display(Name = "SHA-256"), StringLength(64)] string? HashSha256,
        [property: Display(Name = "Vertraulichkeit"), StringLength(64)] string? Confidentiality
    );
}
