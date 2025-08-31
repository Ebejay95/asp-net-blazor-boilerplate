using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Risk
{
    /// <summary>Compute risk for ONE scenario. If OverrideRevenuePerYear is null, use the scenario's customer's RevenuePerYear.</summary>
    public sealed record ComputeScenarioRiskRequest(
        [property: Required] Guid ScenarioId,
        decimal? OverrideRevenuePerYear = null
    );
}
