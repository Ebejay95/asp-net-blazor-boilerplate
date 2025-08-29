using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Frameworks;

/// <summary>
/// Vollständiges DTO für Framework-Anzeigen und -Listen.
/// </summary>
public record FrameworkDto(
	[property: ScaffoldColumn(false)]
	Guid Id,

	[property: Display(Name = "Framework")]
	string Name,

	[property: Display(Name = "Version")]
	string Version,

	[property: Display(Name = "Branche")]
	string Industry,

	[property: Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
	DateTime CreatedAt,

	[property: Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
	DateTime UpdatedAt
);
