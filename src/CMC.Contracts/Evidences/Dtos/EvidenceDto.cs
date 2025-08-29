using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Evidences
{
	public class EvidenceDto
	{
		[ScaffoldColumn(false)]
		public Guid Id { get; set; }

		[ScaffoldColumn(false)]
		public Guid CustomerId { get; set; }

		[Display(Name = "Quelle")]
		public string Source { get; set; } = string.Empty;

		[Display(Name = "Ablageort")]
		public string Location { get; set; } = string.Empty;

		[Display(Name = "Erfasst am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTime CollectedAt { get; set; }

		[Display(Name = "Gültig bis"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTime? ValidUntil { get; set; }

		[Display(Name = "SHA-256")]
		public string HashSha256 { get; set; } = string.Empty;

		[Display(Name = "Vertraulichkeit")]
		public string Confidentiality { get; set; } = string.Empty;

		[Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTime CreatedAt { get; set; }

		[Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTime UpdatedAt { get; set; }
	}
}
