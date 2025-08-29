using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Controls
{
	public record ChangeControlStatusRequest(
		[property: Required] Guid ControlId,
		[property: Required, StringLength(50)] string NewStatus,
		[property: Required] DateTimeOffset AsOfUtc
	);
}
