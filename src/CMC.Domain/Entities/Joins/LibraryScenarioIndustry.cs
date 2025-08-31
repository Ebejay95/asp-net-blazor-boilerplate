using System;

namespace CMC.Domain.Entities
{
	public class LibraryScenarioIndustry
	{
		public Guid LibraryScenarioId { get; private set; }
		public Guid IndustryId { get; private set; }
		public virtual LibraryScenario LibraryScenario { get; private set; } = null!;
		public virtual Industry Industry { get; private set; } = null!;
		private LibraryScenarioIndustry() { }
		public LibraryScenarioIndustry(Guid libScenarioId, Guid industryId) { LibraryScenarioId = libScenarioId; IndustryId = industryId; }
	}
}
