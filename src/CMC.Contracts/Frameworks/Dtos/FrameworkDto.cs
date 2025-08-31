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

		// 👉 editierbare M:N IDs
		[Display(Name = "Branchen")]
		public IReadOnlyList<Guid> IndustryIds { get; set; } = Array.Empty<Guid>();

		// 👉 reine Anzeige
		[EditorHidden]
		[Display(Name = "Branchen (Namen)")]
		public IReadOnlyList<string> IndustryNames { get; set; } = Array.Empty<string>();

		[Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset CreatedAt { get; set; }

		[Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTimeOffset UpdatedAt { get; set; }
	}
}
