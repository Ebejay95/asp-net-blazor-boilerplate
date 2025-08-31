using System;

namespace CMC.Domain.Entities
{
	public class LibraryScenarioTag
	{
		public Guid LibraryScenarioId { get; private set; }
		public Guid TagId { get; private set; }
		public virtual LibraryScenario LibraryScenario { get; private set; } = null!;
		public virtual Tag Tag { get; private set; } = null!;
		private LibraryScenarioTag() { }
		public LibraryScenarioTag(Guid libScenarioId, Guid tagId) { LibraryScenarioId = libScenarioId; TagId = tagId; }
	}
}
