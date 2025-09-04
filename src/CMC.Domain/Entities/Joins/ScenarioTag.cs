using System;

namespace CMC.Domain.Entities
{
	public class ScenarioTag
	{
		public Guid ScenarioId { get; private set; }
		public Guid TagId { get; private set; }
		public virtual Scenario Scenario { get; private set; } = null!;
		public virtual Tag Tag { get; private set; } = null!;
		private ScenarioTag() { }
		public ScenarioTag(Guid scenarioId, Guid tagId) { ScenarioId = scenarioId; TagId = tagId; }
	}
}
