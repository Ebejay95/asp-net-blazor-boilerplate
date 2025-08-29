using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Frameworks;

/// <summary>
/// Request zum Löschen eines Framework-Eintrags.
/// </summary>
public record DeleteFrameworkRequest(
	[property: Required]
	Guid Id
);
