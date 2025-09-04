using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.LibraryControls
{
    public record UpdateLibraryControlRequest(
        [property: Required] Guid Id,
        [property: Range(0, int.MaxValue), Display(Name = "Interne Tage")]
        int InternalDays,
        [property: Range(0, int.MaxValue), Display(Name = "Externe Tage")]
        int ExternalDays,
        int? TotalDays,
        [property: Range(typeof(decimal), "0", "79228162514264337593543950335"), Display(Name = "Capex (EUR)")]
        decimal CapexEur,
        [property: Range(typeof(decimal), "0", "79228162514264337593543950335"), Display(Name = "Opex/Jahr (EUR)")]
        decimal OpexYearEur,

        [property: Display(Name = "Tags")]
        IReadOnlyList<Guid>? TagIds = null,
        [property: Display(Name = "Branchen")]
        IReadOnlyList<Guid>? IndustryIds = null,
        [property: Display(Name = "Szenarien")]
        IReadOnlyList<Guid>? LibraryScenarioIds = null
    );
}
