using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CMC.Contracts.Common;

namespace CMC.Contracts.LibraryScenarios
{
	public class LibraryScenarioDto
	{
		[ScaffoldColumn(false)]
		public Guid Id { get; set; }

		[Display(Name = "Szenario")]
		public string Name { get; set; } = string.Empty;

		[Display(Name = "Jährliche Häufigkeit")]
		public decimal AnnualFrequency { get; set; }

		[Display(Name = "Impact (% Umsatz)"), DisplayFormat(DataFormatString = "{0:P2}")]
		public decimal ImpactPctRevenue { get; set; }

		// M:N – Tags (IDs editierbar)
[Display(Name = "Tags", AutoGenerateField = false)]
[RelationFrom(IsMany = true, RelationName = "TagLinks")]
public IReadOnlyList<Guid> TagIds { get; set; } = Array.Empty<Guid>();

		// nur fürs Grid
		[EditorHidden]
		[Display(Name = "Tag-Namen")]
		public IReadOnlyList<string> TagLabels { get; set; } = Array.Empty<string>();

[Display(Name = "Branchen", AutoGenerateField = false)]
[RelationFrom(IsMany = true, RelationName = "IndustryLinks")]   // <- WICHTIG
public IReadOnlyList<Guid> IndustryIds { get; set; } = Array.Empty<Guid>();

		// nur fürs Grid
		[EditorHidden]
		[Display(Name = "Branchen-Namen")]
		public IReadOnlyList<string> IndustryLabels { get; set; } = Array.Empty<string>();

		[Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset CreatedAt { get; set; }

		[Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset UpdatedAt { get; set; }
	}
}
