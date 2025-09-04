using System;

namespace CMC.Domain.Entities
{
    public class ControlIndustry
    {
        public Guid ControlId { get; private set; }
        public Guid IndustryId { get; private set; }

        public virtual Control Control { get; private set; } = null!;
        public virtual Industry Industry { get; private set; } = null!;

        private ControlIndustry() { }
        public ControlIndustry(Guid controlId, Guid industryId)
        {
            if (controlId == Guid.Empty) throw new ArgumentException(nameof(controlId));
            if (industryId == Guid.Empty) throw new ArgumentException(nameof(industryId));
            ControlId = controlId;
            IndustryId = industryId;
        }
    }
}
