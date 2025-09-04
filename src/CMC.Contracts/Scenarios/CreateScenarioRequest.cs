using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Scenarios
{
    public record CreateScenarioRequest(
        [property: Required] Guid CustomerId,
        [property: Required] Guid LibraryScenarioId,
        [property: Required, StringLength(200, MinimumLength = 1)] string Name,
        [property: Required] decimal AnnualFrequency,
        [property: Required] decimal ImpactPctRevenue,
        [property: Display(Name = "Tags")] IReadOnlyList<Guid>? TagIds = null
    );
}
