using System;
using System.Collections.Generic;

namespace CMC.Domain.Entities
{
	public static class ControlStatus
	{
		public const string Proposed   = "proposed";
		public const string Planned    = "planned";
		public const string InProgress = "in_progress";
		public const string Blocked    = "blocked";
		public const string Active     = "active";
		public const string Retired    = "retired";

		public static readonly ISet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			Proposed, Planned, InProgress, Blocked, Active, Retired
		};

		// erlaubt → Zielmengen
		public static readonly IDictionary<string, ISet<string>> AllowedTransitions =
			new Dictionary<string, ISet<string>>(StringComparer.OrdinalIgnoreCase)
			{
				[Proposed]   = new HashSet<string>(StringComparer.OrdinalIgnoreCase){ Planned, InProgress, Active, Retired },
				[Planned]    = new HashSet<string>(StringComparer.OrdinalIgnoreCase){ InProgress, Blocked, Retired },
				[InProgress] = new HashSet<string>(StringComparer.OrdinalIgnoreCase){ Active, Blocked, Retired },
				[Blocked]    = new HashSet<string>(StringComparer.OrdinalIgnoreCase){ InProgress, Retired },
				[Active]     = new HashSet<string>(StringComparer.OrdinalIgnoreCase){ Retired },
				[Retired]    = new HashSet<string>(StringComparer.OrdinalIgnoreCase){ InProgress, Active } // Re-Enable möglich
			};
	}
}
