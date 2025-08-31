using System;

namespace CMC.Domain.Entities
{
	public class LibraryControlScenario
	{
		public Guid LibraryControlId { get; private set; }
		public Guid LibraryScenarioId { get; private set; }
		public virtual LibraryControl LibraryControl { get; private set; } = null!;
		public virtual LibraryScenario LibraryScenario { get; private set; } = null!;
		private LibraryControlScenario() { }
		public LibraryControlScenario(Guid libControlId, Guid libScenarioId) { LibraryControlId = libControlId; LibraryScenarioId = libScenarioId; }
	}
}
