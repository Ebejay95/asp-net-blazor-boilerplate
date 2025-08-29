using System;

namespace CMC.Domain.Entities
{
	public class LibraryControlFramework
	{
		public Guid ControlId { get; private set; }
		public Guid FrameworkId { get; private set; }

		public virtual LibraryControl Control { get; private set; } = null!;
		public virtual Framework Framework { get; private set; } = null!;

		private LibraryControlFramework() { }

		public LibraryControlFramework(Guid controlId, Guid frameworkId)
		{
			ControlId = controlId;
			FrameworkId = frameworkId;
		}
	}
}
