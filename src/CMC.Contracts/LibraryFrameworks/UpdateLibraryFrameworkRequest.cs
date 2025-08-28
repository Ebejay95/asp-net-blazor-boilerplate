using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.LibraryFrameworks;

/// <summary>
/// Request zum Aktualisieren eines bestehenden Framework-Eintrags.
/// </summary>
public record UpdateLibraryFrameworkRequest(
	[Required]
	Guid Id,

	[Required]
	[StringLength(200, MinimumLength = 1)]
	[Display(Name = "Framework")]
	string Name,

	[Required]
	[StringLength(64, MinimumLength = 1)]
	[Display(Name = "Version")]
	string Version,

	[StringLength(100)]
	[Display(Name = "Branche")]
	string? Industry = null
);
