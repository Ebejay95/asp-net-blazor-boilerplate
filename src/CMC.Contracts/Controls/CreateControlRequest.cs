using System;
using System.ComponentModel.DataAnnotations;
using CMC.Contracts.Common;

namespace CMC.Contracts.Controls
{
    public record CreateControlRequest(
        [property: Required] Guid CustomerId,
        [property: Required] Guid LibraryControlId,

        [property: Display(Name = "Umgesetzt")] bool Implemented,
        [property: Range(0, 1)] decimal Coverage,
        [property: Display(Name = "Maturity")] int Maturity,
        [property: Range(0, 1)] decimal EvidenceWeight,
        Guid? EvidenceId,
        [property: Range(0, 1)] decimal Freshness,

        [property: Display(Name = "Kosten (EUR)")] decimal CostTotalEur,
        // Delta & Score können serverseitig berechnet/überschrieben werden – bleiben im Contract weg
        //[property: Display(Name = "ΔEAL (EUR)")] decimal DeltaEalEur,
        //[property: Display(Name = "Score")] decimal Score,

        // Optionaler Initialstatus als Tag (wird beim Anlegen via Transition-Logik validiert)
        [property: Display(Name = "Status")]
        [property: SelectFrom("CMC.Contracts.Controls.ControlStatuses.Statuses")]
        string? InitialStatusTag = null,

        [property: Display(Name = "Fällig am")] DateTimeOffset? DueDate = null
    );
}
