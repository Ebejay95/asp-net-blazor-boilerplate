using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.LibraryFrameworks;

/// <summary>
/// Request zum Löschen eines Framework-Eintrags.
/// </summary>
public record DeleteLibraryFrameworkRequest(
	[property: Required]
	Guid Id
);
