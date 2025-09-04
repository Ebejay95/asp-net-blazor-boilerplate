using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CMC.Contracts.Common;

namespace CMC.Contracts.LibraryControls
{
    public class LibraryControlDto
    {
        [ScaffoldColumn(false)]
        public Guid Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Capex (EUR)"), DisplayFormat(DataFormatString = "{0:C}")]
        public decimal CapexEur { get; set; }

        [Display(Name = "Opex/Jahr (EUR)"), DisplayFormat(DataFormatString = "{0:C}")]
        public decimal OpexYearEur { get; set; }

        [Display(Name = "Interne Tage")]
        public int InternalDays { get; set; }

        [Display(Name = "Externe Tage")]
        public int ExternalDays { get; set; }

        [Display(Name = "Gesamt Tage")]
        [EditorHidden]
        public int TotalDays { get; set; }

        // Tags
        [Display(Name = "Tags", AutoGenerateField = false)]
        [RelationFrom(IsMany = true, RelationName = "TagLinks")]
        public IReadOnlyList<Guid> TagIds { get; set; } = Array.Empty<Guid>();

        [EditorHidden]
        [Display(Name = "Tags (Namen)")]
        public IReadOnlyList<string> TagLabels { get; set; } = Array.Empty<string>();

        // Branchen
        [Display(Name = "Branchen", AutoGenerateField = false)]
        [RelationFrom(IsMany = true, RelationName = "IndustryLinks")]
        public IReadOnlyList<Guid> IndustryIds { get; set; } = Array.Empty<Guid>();

        [EditorHidden]
        [Display(Name = "Branchen (Namen)")]
        public IReadOnlyList<string> IndustryLabels { get; set; } = Array.Empty<string>();

        // Library-Szenarien
        [Display(Name = "Szenarien", AutoGenerateField = false)]
        [RelationFrom(IsMany = true, RelationName = "ScenarioLinks")]
        public IReadOnlyList<Guid> LibraryScenarioIds { get; set; } = Array.Empty<Guid>();

        [EditorHidden]
        [Display(Name = "Szenarien (Namen)")]
        public IReadOnlyList<string> LibraryScenarioLabels { get; set; } = Array.Empty<string>();

        [Display(Name = "Erstellt am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTimeOffset CreatedAt { get; set; }

        [Display(Name = "Aktualisiert am"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
