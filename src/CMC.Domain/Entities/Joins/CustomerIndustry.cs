using System;

namespace CMC.Domain.Entities.Joins
{
	// Explizite Join-Entity (ermöglicht später Zusatzspalten wie CreatedBy, Weight, etc.)
	public class CustomerIndustry
	{
		public Guid CustomerId { get; private set; }
		public Guid IndustryId { get; private set; }

		public virtual Entities.Customer Customer { get; private set; } = null!;
		public virtual Entities.Industry Industry { get; private set; } = null!;

		private CustomerIndustry() { }

		public CustomerIndustry(Guid customerId, Guid industryId)
		{
			if (customerId == Guid.Empty) throw new ArgumentException(nameof(customerId));
			if (industryId == Guid.Empty) throw new ArgumentException(nameof(industryId));

			CustomerId = customerId;
			IndustryId = industryId;
		}
	}
}
