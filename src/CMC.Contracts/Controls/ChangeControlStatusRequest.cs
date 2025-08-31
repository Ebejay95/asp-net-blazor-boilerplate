using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Controls
{
    /// <summary>Status-Transition für einen Control (nutzt Domain: Control.TransitionTo(...)).</summary>
    public sealed record ChangeControlStatusRequest(
        [property: Required] Guid ControlId,
        [property: Required, StringLength(64, MinimumLength = 1)] string NewStatus,
        // Optional: Zeitpunkt der Transition (UTC). Wenn null, nimmt die Domain "jetzt".
        DateTimeOffset? AsOfUtc = null
    );
}
