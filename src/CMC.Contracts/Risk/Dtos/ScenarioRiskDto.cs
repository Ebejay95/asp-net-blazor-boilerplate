using System;

namespace CMC.Contracts.Risk
{
    /// <summary>Full result for one scenario.</summary>
    public sealed record ScenarioRiskDto(
        Guid ScenarioId,
        Guid CustomerId,
        string ScenarioName,
        decimal AnnualFrequency,
        decimal ImpactPctRevenue,
        decimal RevenuePerYear,
        decimal BaseEal,
        decimal ResidualFrequency,
        decimal ResidualImpactPct,
        decimal ResidualEal,
        decimal DeltaEal
    );
}
