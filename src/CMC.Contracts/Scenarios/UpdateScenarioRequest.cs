using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Scenarios
{
    public record UpdateScenarioRequest(
        [property: Required] Guid Id,
        [property: Required, StringLength(200, MinimumLength = 1)] string Name,
        [property: Required] decimal AnnualFrequency,
        [property: Required] decimal ImpactPctRevenue,
        [property: Display(Name = "Tags")] IReadOnlyList<Guid>? TagIds = null
    );
}
