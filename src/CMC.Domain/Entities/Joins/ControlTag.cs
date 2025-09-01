using System;

namespace CMC.Domain.Entities
{
    public class ControlTag
    {
        public Guid ControlId { get; private set; }
        public Guid TagId { get; private set; }

        public virtual Control Control { get; private set; } = null!;
        public virtual Tag Tag { get; private set; } = null!;

        private ControlTag() { }
        public ControlTag(Guid controlId, Guid tagId)
        {
            if (controlId == Guid.Empty) throw new ArgumentException(nameof(controlId));
            if (tagId == Guid.Empty) throw new ArgumentException(nameof(tagId));
            ControlId = controlId;
            TagId = tagId;
        }
    }
}
