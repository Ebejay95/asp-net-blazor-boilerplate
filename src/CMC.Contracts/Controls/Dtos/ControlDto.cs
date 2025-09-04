using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CMC.Contracts.Common;

namespace CMC.Contracts.Controls
{
    public class ControlDto
    {
        [ScaffoldColumn(false)]
        public Guid Id { get; set; }

		[Required, StringLength(200, MinimumLength = 1)]
		[Display(Name = "Tag")]
		public string Name { get; set; } = string.Empty;

        [Display(Name = "Kunde", AutoGenerateField = false)]
        [RelationFrom(IsMany = false, RelationName = "Customer")]
        public Guid CustomerId { get; set; }

        [Display(Name = "Vorlage", AutoGenerateField = false)]
        [RelationFrom(IsMany = false, RelationName = "LibraryControl")]
        public Guid LibraryControlId { get; set; }

        [Display(Name = "Evidence")]
        public Guid? EvidenceId { get; set; }

        [Display(Name = "Umgesetzt")]
        public bool Implemented { get; set; }

        [Range(0, 1)]
        [Display(Name = "Coverage")]
        public decimal Coverage { get; set; }

        [Display(Name = "Maturity")]
        public int Maturity { get; set; }

        [Range(0, 1)]
        [Display(Name = "Evidence Weight")]
        public decimal EvidenceWeight { get; set; }

        [Range(0, 1)]
        [Display(Name = "Freshness")]
        public decimal Freshness { get; set; }

        [Display(Name = "Kosten (EUR)"), DisplayFormat(DataFormatString = "{0:C}")]
        public decimal CostTotalEur { get; set; }

        [Display(Name = "ΔEAL (EUR)"), DisplayFormat(DataFormatString = "{0:C}")]
        public decimal DeltaEalEur { get; set; }

        [Display(Name = "Score")]
        public decimal Score { get; set; }

        [EditorHidden]
        [Display(Name = "Status (Label)")]
        public string Status { get; set; } = string.Empty;

        [SelectFrom("CMC.Contracts.Controls.ControlStatuses.Statuses")]
        [Display(Name = "Status")]
        public string? StatusTag { get; set; }

        [Display(Name = "Fällig am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTimeOffset? DueDate { get; set; }

        // ===== M:N Felder (Checkboxen) =====
        [Display(Name = "Tags", AutoGenerateField = false)]
        [RelationFrom(IsMany = true, RelationName = "TagLinks")]
        public IReadOnlyList<Guid> TagIds { get; set; } = Array.Empty<Guid>();

        [EditorHidden]
        [Display(Name = "Tags (Namen)")]
        public IReadOnlyList<string> TagLabels { get; set; } = Array.Empty<string>();

        [Display(Name = "Branchen", AutoGenerateField = false)]
        [RelationFrom(IsMany = true, RelationName = "IndustryLinks")]
        public IReadOnlyList<Guid> IndustryIds { get; set; } = Array.Empty<Guid>();

        [EditorHidden]
        [Display(Name = "Branchen (Namen)")]
        public IReadOnlyList<string> IndustryLabels { get; set; } = Array.Empty<string>();

        [Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTimeOffset CreatedAt { get; set; }

        [Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
