using System;

namespace CMC.Domain.Entities
{
	public class LibraryControlIndustry
	{
		public Guid ControlId { get; private set; }
		public Guid IndustryId { get; private set; }

		public virtual LibraryControl Control { get; private set; } = null!;
		public virtual Industry Industry { get; private set; } = null!;

		private LibraryControlIndustry() { }

		public LibraryControlIndustry(Guid controlId, Guid industryId)
		{
			ControlId = controlId;
			IndustryId = industryId;
		}
	}
}
