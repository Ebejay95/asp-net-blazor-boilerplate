using System;

namespace CMC.Domain.Entities
{
	public class LibraryControlFramework
	{
		public Guid LibraryControlId { get; private set; }
		public Guid FrameworkId { get; private set; }
		public virtual LibraryControl LibraryControl { get; private set; } = null!;
		public virtual Framework Framework { get; private set; } = null!;
		private LibraryControlFramework() { }
		public LibraryControlFramework(Guid libControlId, Guid frameworkId) { LibraryControlId = libControlId; FrameworkId = frameworkId; }
	}
}
