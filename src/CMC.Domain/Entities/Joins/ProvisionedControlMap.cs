using System;

namespace CMC.Domain.Entities
{
    /// <summary>
    /// Mapping für Idempotenz: Kundenspezifisches Control aus LibraryControl für eine konkrete kundenspezifische Scenario.
    /// </summary>
    public class ProvisionedControlMap
    {
        public Guid CustomerId { get; private set; }
        public Guid LibraryControlId { get; private set; }
        public Guid ScenarioId { get; private set; } // kundenspezifisches Scenario
        public Guid ControlId { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        private ProvisionedControlMap() { }

        public ProvisionedControlMap(Guid customerId, Guid libraryControlId, Guid scenarioId, Guid controlId, DateTimeOffset? createdAtUtc = null)
        {
            if (customerId == Guid.Empty) throw new ArgumentException(nameof(customerId));
            if (libraryControlId == Guid.Empty) throw new ArgumentException(nameof(libraryControlId));
            if (scenarioId == Guid.Empty) throw new ArgumentException(nameof(scenarioId));
            if (controlId == Guid.Empty) throw new ArgumentException(nameof(controlId));

            CustomerId = customerId;
            LibraryControlId = libraryControlId;
            ScenarioId = scenarioId;
            ControlId = controlId;
            CreatedAt = (createdAtUtc ?? DateTimeOffset.UtcNow).ToUniversalTime();
        }
    }
}
