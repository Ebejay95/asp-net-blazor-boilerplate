using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Frameworks;

/// <summary>
/// Request zum Aktualisieren eines bestehenden Framework-Eintrags.
/// </summary>
public record UpdateFrameworkRequest(
	[property: Required]
	Guid Id,

	[property: Required]
	[property: StringLength(200, MinimumLength = 1)]
	[property: Display(Name = "Framework")]
	string Name,

	[property: Required]
	[property: StringLength(64, MinimumLength = 1)]
	[property: Display(Name = "Version")]
	string Version,

	[property: Display(Name = "Branchen")]
	IReadOnlyList<Guid>? IndustryIds = null
);
