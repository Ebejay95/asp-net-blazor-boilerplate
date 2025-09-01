using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CMC.Contracts.Common; // ✅ needed for SelectFrom

namespace CMC.Contracts.Controls
{
    public record UpdateControlRequest(
        [property: Required] Guid Id,
        [property: Display(Name = "Umgesetzt")] bool Implemented,
        [property: Range(0, 1)] decimal Coverage,
        [property: Display(Name = "Maturity")] int Maturity,
        [property: Range(0, 1)] decimal EvidenceWeight,
        [property: Range(0, 1)] decimal Freshness,
        [property: Display(Name = "Kosten (EUR)")] decimal CostTotalEur,
        [property: Display(Name = "ΔEAL (EUR)")] decimal DeltaEalEur,
        [property: Display(Name = "Score")] decimal Score,

        // Status per Tag (Transition)
        [property: Display(Name = "Status")]
        [property: SelectFrom("CMC.Contracts.Controls.ControlStatuses.Statuses")]
        string? StatusTag,

        DateTimeOffset? DueDate,
        Guid? EvidenceId,

        // M:N
        [property: Display(Name = "Tags")]     IReadOnlyList<Guid>? TagIds = null,
        [property: Display(Name = "Branchen")] IReadOnlyList<Guid>? IndustryIds = null
    );
}
