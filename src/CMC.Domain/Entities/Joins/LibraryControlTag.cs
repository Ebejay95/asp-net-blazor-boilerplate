using System;

namespace CMC.Domain.Entities
{
	public class LibraryControlTag
	{
		public Guid LibraryControlId { get; private set; }
		public Guid TagId { get; private set; }
		public virtual LibraryControl LibraryControl { get; private set; } = null!;
		public virtual Tag Tag { get; private set; } = null!;
		private LibraryControlTag() { }
		public LibraryControlTag(Guid libControlId, Guid tagId) { LibraryControlId = libControlId; TagId = tagId; }
	}
}
