using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Risk
{
    /// <summary>Compute risk for MANY scenarios of a customer (optionally filtered).</summary>
    public sealed record ComputeCustomerRiskRequest(
        [property: Required] Guid CustomerId,
        IReadOnlyList<Guid>? ScenarioIds = null,
        decimal? OverrideRevenuePerYear = null
    );
}
