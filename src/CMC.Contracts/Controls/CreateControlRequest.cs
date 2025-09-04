using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CMC.Contracts.Common; // ✅ needed for SelectFrom

namespace CMC.Contracts.Controls
{
    public record CreateControlRequest(
        [property: Required, StringLength(200, MinimumLength = 1), Display(Name = "Tag")]
		string Name,
        [property: Required] Guid CustomerId,
        [property: Required] Guid LibraryControlId,

        [property: Display(Name = "Umgesetzt")] bool Implemented,
        [property: Range(0, 1)] decimal Coverage,
        [property: Display(Name = "Maturity")] int Maturity,
        [property: Range(0, 1)] decimal EvidenceWeight,
        Guid? EvidenceId,
        [property: Range(0, 1)] decimal Freshness,

        [property: Display(Name = "Kosten (EUR)")] decimal CostTotalEur,

        // Optionaler Initialstatus (Tag)
        [property: Display(Name = "Status")]
        [property: SelectFrom("CMC.Contracts.Controls.ControlStatuses.Statuses")]
        string? InitialStatusTag = null,

        [property: Display(Name = "Fällig am")] DateTimeOffset? DueDate = null,

        // M:N
        [property: Display(Name = "Tags")]     IReadOnlyList<Guid>? TagIds = null,
        [property: Display(Name = "Branchen")] IReadOnlyList<Guid>? IndustryIds = null
    );
}
