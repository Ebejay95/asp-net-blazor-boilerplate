using System;
using System.ComponentModel.DataAnnotations;
using CMC.Contracts.Common;

namespace CMC.Contracts.Controls
{
    /// <summary>
    /// Aktualisiert Felder des Controls. Wenn <see cref="StatusTag"/> angegeben ist,
    /// wird serverseitig eine Transition ausgeführt (nicht “roh” gesetzt).
    /// </summary>
    public record UpdateControlRequest(
        [property: Required] Guid Id,
        [property: Display(Name = "Umgesetzt")] bool Implemented,
        [property: Range(0, 1)] decimal Coverage,
        [property: Display(Name = "Maturity")] int Maturity,
        [property: Range(0, 1)] decimal EvidenceWeight,
        [property: Range(0, 1)] decimal Freshness,
        [property: Display(Name = "Kosten (EUR)")] decimal CostTotalEur,

        // ΔEAL & Score lässt du idealerweise vom Backend berechnen. Falls du manuell setzen willst:
        [property: Display(Name = "ΔEAL (EUR)")] decimal DeltaEalEur,
        [property: Display(Name = "Score")] decimal Score,

        // Optional: Status-Transition per Tag (proposed/planned/...).
        [property: Display(Name = "Status")]
        [property: SelectFrom("CMC.Contracts.Controls.ControlStatuses.Statuses")]
        string? StatusTag,

        DateTimeOffset? DueDate,
        Guid? EvidenceId
    );
}
