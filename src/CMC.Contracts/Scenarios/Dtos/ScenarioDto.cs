using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Scenarios
{
	public class ScenarioDto
	{
		[ScaffoldColumn(false)]
		public Guid Id { get; set; }

		[ScaffoldColumn(false)]
		public Guid CustomerId { get; set; }

		[ScaffoldColumn(false)]
		public Guid LibraryScenarioId { get; set; }

		[Display(Name = "Szenario")]
		public string Name { get; set; } = string.Empty;

		[Display(Name = "Jährliche Häufigkeit")]
		public decimal AnnualFrequency { get; set; }

		[Display(Name = "Impact (% Umsatz)"), DisplayFormat(DataFormatString = "{0:P2}")]
		public decimal ImpactPctRevenue { get; set; }

		[Display(Name = "Tags")]
		public string Tags { get; set; } = string.Empty;

		[Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTime CreatedAt { get; set; }

		[Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTime UpdatedAt { get; set; }
	}
}
