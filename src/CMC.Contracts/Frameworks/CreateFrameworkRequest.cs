using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Frameworks;

/// <summary>
/// Request zum Anlegen eines neuen Framework-Eintrags.
/// </summary>
public record CreateFrameworkRequest(
	[property: Required]
	[property: StringLength(200, MinimumLength = 1)]
	[property: Display(Name = "Framework")]
	string Name,

	[property: Required]
	[property: StringLength(64, MinimumLength = 1)]
	[property: Display(Name = "Version")]
	string Version,

	// M:N: mehrere Branchen
	[property: Display(Name = "Branchen")]
	IReadOnlyList<Guid>? IndustryIds = null
);
