using System;

namespace CMC.Domain.Entities
{
	public class LibraryScenarioIndustry
	{
		public Guid ScenarioId { get; private set; }
		public Guid IndustryId { get; private set; }

		public virtual LibraryScenario Scenario { get; private set; } = null!;
		public virtual Industry Industry { get; private set; } = null!;

		private LibraryScenarioIndustry() { }

		public LibraryScenarioIndustry(Guid scenarioId, Guid industryId)
		{
			ScenarioId = scenarioId;
			IndustryId = industryId;
		}
	}
}
