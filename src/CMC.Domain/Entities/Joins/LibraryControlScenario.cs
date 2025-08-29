using System;

namespace CMC.Domain.Entities
{
	public class LibraryControlScenario
	{
		public Guid ControlId { get; private set; }
		public Guid ScenarioId { get; private set; }

		// Effekte pro Relation (freq_eff / impact_eff)
		public decimal FrequencyEffect { get; private set; }
		public decimal ImpactEffect { get; private set; }

		public virtual LibraryControl Control { get; private set; } = null!;
		public virtual LibraryScenario Scenario { get; private set; } = null!;

		private LibraryControlScenario() { }

		public LibraryControlScenario(Guid controlId, Guid scenarioId, decimal frequencyEffect, decimal impactEffect)
		{
			ControlId = controlId;
			ScenarioId = scenarioId;
			FrequencyEffect = frequencyEffect;
			ImpactEffect = impactEffect;
		}

		public void UpdateEffects(decimal frequencyEffect, decimal impactEffect)
		{
			FrequencyEffect = frequencyEffect;
			ImpactEffect = impactEffect;
		}
	}
}
