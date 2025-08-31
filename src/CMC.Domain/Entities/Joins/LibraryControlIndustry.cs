using System;

namespace CMC.Domain.Entities
{
	public class LibraryControlIndustry
	{
		public Guid LibraryControlId { get; private set; }
		public Guid IndustryId { get; private set; }
		public virtual LibraryControl LibraryControl { get; private set; } = null!;
		public virtual Industry Industry { get; private set; } = null!;
		private LibraryControlIndustry() { }
		public LibraryControlIndustry(Guid libControlId, Guid industryId) { LibraryControlId = libControlId; IndustryId = industryId; }
	}
}
