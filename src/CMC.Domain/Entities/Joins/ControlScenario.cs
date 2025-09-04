using System;

namespace CMC.Domain.Entities
{
    public class ControlScenario
    {
        public Guid ControlId { get; private set; }
        public Guid ScenarioId { get; private set; }

        public virtual Control Control { get; private set; } = null!;
        public virtual Scenario Scenario { get; private set; } = null!;

        private ControlScenario() { }
        public ControlScenario(Guid controlId, Guid scenarioId)
        {
            if (controlId == Guid.Empty) throw new ArgumentException(nameof(controlId));
            if (scenarioId == Guid.Empty) throw new ArgumentException(nameof(scenarioId));
            ControlId = controlId;
            ScenarioId = scenarioId;
        }
    }
}
