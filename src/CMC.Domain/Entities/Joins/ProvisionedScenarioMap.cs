using System;

namespace CMC.Domain.Entities
{
    /// <summary>
    /// Mapping für Idempotenz: Kundenspezifisches Scenario aus LibraryScenario.
    /// </summary>
    public class ProvisionedScenarioMap
    {
        public Guid CustomerId { get; private set; }
        public Guid LibraryScenarioId { get; private set; }
        public Guid ScenarioId { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        private ProvisionedScenarioMap() { }

        public ProvisionedScenarioMap(Guid customerId, Guid libraryScenarioId, Guid scenarioId, DateTimeOffset? createdAtUtc = null)
        {
            if (customerId == Guid.Empty) throw new ArgumentException(nameof(customerId));
            if (libraryScenarioId == Guid.Empty) throw new ArgumentException(nameof(libraryScenarioId));
            if (scenarioId == Guid.Empty) throw new ArgumentException(nameof(scenarioId));

            CustomerId = customerId;
            LibraryScenarioId = libraryScenarioId;
            ScenarioId = scenarioId;
            CreatedAt = (createdAtUtc ?? DateTimeOffset.UtcNow).ToUniversalTime();
        }
    }
}
