using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CMC.Contracts.Common;

namespace CMC.Contracts.Frameworks
{
	public class FrameworkDto
	{
		[ScaffoldColumn(false)]
		public Guid Id { get; set; }

		[Display(Name = "Framework")]
		public string Name { get; set; } = string.Empty;

		[Display(Name = "Version")]
		public string Version { get; set; } = string.Empty;

		[Display(Name = "Branchen", AutoGenerateField = false)]
		[RelationFrom(IsMany = true, RelationName = "IndustryLinks")] // WICHTIG: Parent-Nav!
		public IReadOnlyList<Guid> IndustryIds { get; set; } = Array.Empty<Guid>();


		// Nur für die Tabellenanzeige (lesbare Namen) – im Formular ausblenden.
		[Display(Name = "Branchen")]
		[EditorHidden]
		public IReadOnlyList<string> IndustryNames { get; set; } = Array.Empty<string>();

		[Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset CreatedAt { get; set; }

		[Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset UpdatedAt { get; set; }

		public FrameworkDto() { }

		public FrameworkDto(
			Guid id,
			string name,
			string version,
			IReadOnlyList<Guid> industryIds,
			IReadOnlyList<string> industryNames,
			DateTimeOffset createdAt,
			DateTimeOffset updatedAt)
		{
			Id = id;
			Name = name ?? string.Empty;
			Version = version ?? string.Empty;
			IndustryIds = industryIds ?? Array.Empty<Guid>();
			IndustryNames = industryNames ?? Array.Empty<string>();
			CreatedAt = createdAt;
			UpdatedAt = updatedAt;
		}
	}
}
