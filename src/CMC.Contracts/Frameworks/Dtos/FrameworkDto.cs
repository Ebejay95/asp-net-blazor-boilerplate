using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Frameworks;

/// <summary>
/// Vollständiges DTO für Framework-Anzeigen und -Listen.
/// </summary>
public record FrameworkDto(
	Guid Id,
	string Name,
	string Version,
	IReadOnlyList<Guid> IndustryIds,
	IReadOnlyList<string> IndustryNames,
	DateTimeOffset CreatedAt,
	DateTimeOffset UpdatedAt
);
