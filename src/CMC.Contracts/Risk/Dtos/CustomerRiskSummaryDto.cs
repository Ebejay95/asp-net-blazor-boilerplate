using System;
using System.Collections.Generic;

namespace CMC.Contracts.Risk
{
    /// <summary>Aggregate result for a customer.</summary>
    public sealed record CustomerRiskSummaryDto(
        Guid CustomerId,
        decimal RevenuePerYear,
        decimal TotalBaseEal,
        decimal TotalResidualEal,
        decimal TotalDeltaEal,
        IReadOnlyList<ScenarioRiskDto> Items
    );
}
