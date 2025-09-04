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

        [Display(Name = "Quelle"), StringLength(64, MinimumLength = 1)]
        public string Source { get; set; } = string.Empty;

        [Display(Name = "Ablageort"), StringLength(1024)]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Erfasst am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTimeOffset CollectedAt { get; set; }

        [Display(Name = "Gültig bis"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTimeOffset? ValidUntil { get; set; }

        [Display(Name = "SHA-256"), StringLength(64)]
        public string HashSha256 { get; set; } = string.Empty;

        [Display(Name = "Vertraulichkeit"), StringLength(64)]
        public string Confidentiality { get; set; } = string.Empty;

        [Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTimeOffset CreatedAt { get; set; }

        [Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
