using System;

namespace CMC.Domain.Entities
{
	public class FrameworkIndustry
	{
		public Guid FrameworkId { get; private set; }
		public Guid IndustryId { get; private set; }

		public virtual Framework Framework { get; private set; } = null!;
		public virtual Industry Industry { get; private set; } = null!;

		private FrameworkIndustry() { }

		public FrameworkIndustry(Guid frameworkId, Guid industryId)
		{
			FrameworkId = frameworkId;
			IndustryId = industryId;
		}
	}
}
