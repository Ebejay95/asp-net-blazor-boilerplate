using System;

namespace CMC.Domain.Entities
{
	public class CustomerIndustry
	{
		public Guid CustomerId { get; private set; }
		public Guid IndustryId { get; private set; }

		public virtual Customer Customer { get; private set; } = null!;
		public virtual Industry Industry { get; private set; } = null!;

		private CustomerIndustry() { }

		public CustomerIndustry(Guid customerId, Guid industryId)
		{
			CustomerId = customerId;
			IndustryId = industryId;
		}
	}
}
